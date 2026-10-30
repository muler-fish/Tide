using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Tide.Core;
using Tide.Desktop;

namespace Tide.Desktop.Tests;

internal static class SubscriptionChecks
{
    public static async Task RunAsync(MainWindow owner, string output, Action<Window, string> capture)
    {
        var handler = new CloudHandler(); var store = new LocalStore(Path.Combine(output, "subscription-profile"));
        var vm = new MainViewModel(new XIslandApi(new HttpClient(handler)), store);
        var window = new MainWindow(vm) { Owner = owner, ShowInTaskbar = false };
        try
        {
            window.Show(); vm.SetSubscriptionId("Test-A");
            Require(SecretProtection.Unprotect(store.Load().SubscriptionAccounts.Single(a => a.Id == vm.ActiveSubscriptionKey).EncryptedValue) == "Test-A", "号码未加密保存。");
            Require(!File.ReadAllText(Path.Combine(store.Root, "state.json")).Contains("Test-A"), "明文号码泄露到状态文件。");
            handler.Cloud["Test-A"] = Enumerable.Range(200, 12).Select(i => (long)i).ToList();
            await vm.OpenThreadAsync(211);
            await vm.LoadSubscriptionsAsync();
            Require(vm.SubscriptionKnown && vm.IsSubscribed && vm.SubscriptionCount == "12", "第二页已有订阅被误判为未订阅。");
            await vm.ShowSubscriptionsAsync();
            Require(vm.Posts.Count == 10 && vm.LoadMoreListCommand.CanExecute(null), "订阅续载未启用。");
            await vm.LoadMoreListAsync(); await Pause();
            Require(vm.Posts.Select(p => p.Post.Id).SequenceEqual(Enumerable.Range(200, 12).Select(i => (long)i)), "订阅续页没有保留前一页。");
            capture(window, Path.Combine(output, "subscriptions-page2.png"));
            await vm.ToggleSubscriptionAsync(); Require(!vm.IsSubscribed && !handler.Cloud["Test-A"].Contains(211), "取消订阅没有修改云列表。");
            await vm.ToggleSubscriptionAsync(); Require(vm.IsSubscribed && handler.Cloud["Test-A"].Contains(211), "添加订阅没有修改云列表。");

            // 服务端已写入但回包丢失：下次点击只读取，不会反向取消或自动重发。
            await vm.OpenThreadAsync(999); handler.LoseWriteResponse = true;
            var mutations = handler.Mutations;
            await vm.ToggleSubscriptionAsync();
            Require(!vm.CurrentSubscriptionKnown && handler.Cloud["Test-A"].Contains(999) && handler.Mutations == mutations + 1, "未知结果被当成成功或重试。");
            await vm.ToggleSubscriptionAsync();
            Require(vm.IsSubscribed && handler.Mutations == mutations + 1, "检查未知状态触发了第二次写入。");
            Require(vm.Error.Length == 0, "成功核对状态后仍显示上次订阅错误。");

            handler.FailPageTwo = true; await vm.LoadSubscriptionsAsync(true);
            Require(vm.SubscriptionKnown && vm.Posts.Count > 0 && vm.SubscriptionSyncStatus.StartsWith("缓存"), "刷新失败应保留完整旧快照并标注缓存。");
            handler.FailPageTwo = false;
            handler.HoldNextRead = true; var oldRead = vm.LoadSubscriptionsAsync(true);
            await handler.ReadStarted.Task;
            vm.SetSubscriptionId("Test-B"); handler.Cloud["Test-B"] = [301];
            await vm.LoadSubscriptionsAsync(true); handler.ReleaseRead.TrySetResult(true); await oldRead;
            Require(vm.SubscriptionKnown && vm.SubscriptionCount == "1" && vm.Posts.Single().Post.Id == 301, "切号后旧响应覆盖了新列表。");
            Require(vm.GetSubscriptionId() == "Test-B", "号码切换失败。");
            using (var second = new MainViewModel(new XIslandApi(new HttpClient(new CloudHandler(handler.Cloud))), store))
            {
                await second.OpenThreadAsync(301); Require(second.IsSubscribed, "重启或其他客户端没有读取同号云订阅。");
            }
            vm.State.Theme = "dark"; App.ApplyTheme("dark"); vm.AppearanceChanged();
            await window.Dispatcher.InvokeAsync(window.UpdateLayout, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            await Pause();
            capture(window, Path.Combine(output, "subscriptions-dark.png"));
            var dialog = new SubscriptionWindow(vm) { Owner = window, ShowInTaskbar = false }; dialog.Show(); await Pause();
            Program.RequireWindowFits(dialog); capture(dialog, Path.Combine(output, "subscription-settings-dark.png")); dialog.Close();
            App.ApplyTheme("light"); vm.State.Theme = "light"; vm.AppearanceChanged();
            var light = new SubscriptionWindow(vm) { Owner = window, ShowInTaskbar = false }; light.Show(); await Pause();
            capture(light, Path.Combine(output, "subscription-settings-light.png")); light.Close();
            File.WriteAllText(Path.Combine(output, "subscription-results.json"), JsonSerializer.Serialize(new
            {
                EncryptedId = true, FullPaging = true, CloudAddDelete = true, UnknownReconcilesWithoutWrite = true,
                FailedPageRejectsSnapshot = true, AccountSwitchIgnoresStale = true, ReloadReadsCloud = true
            }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("PASS: encrypted subscription ID, paging, cloud mutations, uncertain result reconciliation, stale response isolation, reload.");
        }
        finally { window.Close(); App.ApplyTheme("light"); }
    }
    private static Task Pause() => Task.Delay(120);
    private static void Require(bool value, string error) { if (!value) throw new InvalidOperationException(error); }
    private sealed class CloudHandler : HttpMessageHandler
    {
        public Dictionary<string, List<long>> Cloud { get; }
        public CloudHandler(Dictionary<string, List<long>>? cloud = null) => Cloud = cloud ?? [];
        public bool LoseWriteResponse, FailPageTwo, HoldNextRead;
        public int Mutations;
        public TaskCompletionSource<bool> ReadStarted { get; } = new();
        public TaskCompletionSource<bool> ReleaseRead { get; } = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            var query = uri.Query.TrimStart('?').Split('&').Select(v => v.Split('=', 2)).ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
            if (uri.AbsolutePath.EndsWith("thread")) return Json(JsonSerializer.Serialize(Post(long.Parse(query["id"]))));
            var id = query["uuid"]; Cloud.TryAdd(id, []);
            if (uri.AbsolutePath.EndsWith("feed"))
            {
                var page = int.Parse(query["page"]);
                var snapshot = Cloud[id].Skip((page - 1) * 10).Take(10).Select(Post).ToArray();
                if (HoldNextRead) { HoldNextRead = false; ReadStarted.SetResult(true); await ReleaseRead.Task; }
                if (FailPageTwo && page == 2) return new(HttpStatusCode.ServiceUnavailable);
                return Json(JsonSerializer.Serialize(snapshot));
            }
            Mutations++; var tid = long.Parse(query["tid"]); var add = uri.AbsolutePath.EndsWith("addFeed");
            Cloud[id].Remove(tid); if (add) Cloud[id].Add(tid);
            if (LoseWriteResponse) { LoseWriteResponse = false; throw new HttpRequestException("synthetic lost response"); }
            return Json(JsonSerializer.Serialize(add ? "订阅大成功" : "取消订阅成功"));
        }
        private static object Post(long id) => new { id, fid = 4, user_hash = "TestPo1", content = "云订阅测试内容", title = "测试串 " + id, reply_count = 1, now = "2026-10-23" };
        private static HttpResponseMessage Json(string value) => new(HttpStatusCode.OK) { Content = new StringContent(value) };
    }
}
