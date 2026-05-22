using System.ComponentModel;
using FileTransferTool.App.Models;
using FileTransferTool.App.Services;

namespace FileTransferTool.App;

public sealed partial class MainForm : Form
{
    // 配置文件沿用旧工具的目录结构，便于直接迁移现有链路配置。
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "Config", "linkdata.xml");
    // 运行时统一托管收发逻辑，界面只负责配置、展示和交互。
    private readonly TransferRuntime _runtime = new();

    private readonly BindingList<SendLinkConfig> _sendLinks = [];
    private readonly BindingList<ReceiveLinkConfig> _receiveLinks = [];
    private readonly BindingList<TransferRow> _activeTransfers = [];
    private readonly BindingList<TransferRow> _historyTransfers = [];
    private readonly BindingList<LogRow> _logRows = [];
    private readonly Dictionary<Guid, TransferRow> _activeIndex = [];
    private readonly Dictionary<string, Panel> _pages = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.OrdinalIgnoreCase);

    public MainForm()
    {
        InitializeComponent();
        InitializePages();
        BindData();

        _runtime.LogWritten += AddLog;
        _runtime.TransferUpdated += HandleTransferUpdated;

        Load += (_, _) => LoadConfig();
        Shown += (_, _) => BeginInvoke(new Action(ApplyResponsiveLayout));
        Resize += (_, _) => ApplyResponsiveLayout();
        FormClosing += (_, _) =>
        {
            SaveConfig();
            _runtime.Dispose();
        };
    }

    private void InitializePages()
    {
        // 左侧导航和右侧页面一一对应，保持旧工具操作习惯，同时让设计器能直接看到控件布局。
        _pages["send-config"] = pageSendConfig;
        _pages["send-state"] = pageSendState;
        _pages["receive-config"] = pageReceiveConfig;
        _pages["receive-state"] = pageReceiveState;
        _pages["all"] = pageAll;

        _navButtons["send-config"] = btnNavSendConfig;
        _navButtons["send-state"] = btnNavSendState;
        _navButtons["receive-config"] = btnNavReceiveConfig;
        _navButtons["receive-state"] = btnNavReceiveState;
        _navButtons["all"] = btnNavAll;

        btnNavSendConfig.Click += (_, _) => ShowPage("send-config");
        btnNavSendState.Click += (_, _) => ShowPage("send-state");
        btnNavReceiveConfig.Click += (_, _) => ShowPage("receive-config");
        btnNavReceiveState.Click += (_, _) => ShowPage("receive-state");
        btnNavAll.Click += (_, _) => ShowPage("all");

        btnMenuSave.Click += (_, _) => SaveConfig();
        btnMenuLoad.Click += (_, _) => LoadConfig();
        btnMenuStartAll.Click += (_, _) => StartAll();
        btnMenuStopAll.Click += (_, _) => StopAll();

        btnSendBrowseMonitor.Click += (_, _) => BrowseFolder(txtSendMonitorPath);
        btnSendBrowseBackup.Click += (_, _) => BrowseFolder(txtSendBackupPath);
        btnReceiveBrowsePath.Click += (_, _) => BrowseFolder(txtReceivePath);

        btnSendAdd.Click += (_, _) => AddSendLink();
        btnSendClear.Click += (_, _) => ClearSendInputs();
        btnSendStart.Click += (_, _) => StartSend();
        btnSendStop.Click += (_, _) => _runtime.StopSend();
        btnSendDelete.Click += (_, _) => DeleteSelectedSend();

        btnReceiveAdd.Click += (_, _) => AddReceiveLink();
        btnReceiveClear.Click += (_, _) => ClearReceiveInputs();
        btnReceiveStart.Click += (_, _) => StartReceive();
        btnReceiveStop.Click += (_, _) => _runtime.StopReceive();
        btnReceiveDelete.Click += (_, _) => DeleteSelectedReceive();

        ShowPage("all");
        ApplyResponsiveLayout();
    }

    private void BindData()
    {
        // 配置表格展示当前链路定义。
        BindSendGrid(dgvSendConfig);
        BindSendGrid(dgvAllSendLinks);
        BindReceiveGrid(dgvReceiveConfig);
        BindReceiveGrid(dgvAllReceiveLinks);

        // 状态表格展示实时流转和最近结果。
        BindTransferGrid(dgvSendState, true);
        BindTransferGrid(dgvReceiveState, false);
        BindHistoryGrid(dgvReceiveHistory);
        BindHistoryGrid(dgvAllHistory);
        BindCombinedTransferGrid(dgvAllActive);
        BindLogGrid(dgvSendLog);
        BindLogGrid(dgvAllLog);

        txtReceiveListenIP.Text = "0.0.0.0";
        numReceiveListenPort.Value = 12092;
        numSendPort.Value = 12092;
    }

    private void ApplyResponsiveLayout()
    {
        // 统一在运行时重新计算每个页面的主区域，避免 Designer 中的绝对坐标把控件都压在左上角。
        grpSendAdd.Dock = DockStyle.None;
        grpSendLinks.Dock = DockStyle.None;
        splitSendState.Dock = DockStyle.None;
        grpReceiveListen.Dock = DockStyle.None;
        grpReceiveAdd.Dock = DockStyle.None;
        grpReceiveLinks.Dock = DockStyle.None;
        splitReceiveState.Dock = DockStyle.None;
        tableAll.Dock = DockStyle.None;

        LayoutSendConfigPage();
        LayoutSendStatePage();
        LayoutReceiveConfigPage();
        LayoutReceiveStatePage();
        LayoutAllPage();
    }

    private void LayoutSendConfigPage()
    {
        const int left = 0;
        const int top = 60;
        const int margin = 12;
        var width = Math.Max(900, pageSendConfig.ClientSize.Width);
        var height = Math.Max(600, pageSendConfig.ClientSize.Height);

        grpSendAdd.SetBounds(left, top, width, 262);
        grpSendLinks.SetBounds(left, top + grpSendAdd.Height + margin, width, Math.Max(220, height - (top + grpSendAdd.Height + margin)));
        LayoutSendConfigContent();
    }

    private void LayoutSendConfigContent()
    {
        const int labelLeft = 18;
        const int inputLeft = 96;
        const int rightLabelLeft = 388;
        const int rightInputLeft = 466;
        const int row1Top = 30;
        const int row2Top = 76;
        const int row3Top = 122;
        const int row4Top = 168;
        const int buttonRowTop = 208;
        const int browseWidth = 76;

        var addWidth = Math.Max(900, grpSendAdd.ClientSize.Width);
        var monitorWidth = Math.Max(260, addWidth - rightInputLeft - browseWidth - 120);
        var backupWidth = Math.Max(360, addWidth - inputLeft - browseWidth - 114);
        var filterWidth = Math.Max(220, addWidth - rightInputLeft - 176);

        lblSendLinkName.Location = new Point(labelLeft, row1Top + 2);
        txtSendLinkName.Location = new Point(inputLeft, row1Top);
        txtSendLinkName.Size = new Size(260, 23);

        lblSendMonitorPath.Location = new Point(rightLabelLeft, row1Top + 2);
        txtSendMonitorPath.Location = new Point(rightInputLeft, row1Top);
        txtSendMonitorPath.Size = new Size(monitorWidth, 23);
        btnSendBrowseMonitor.Location = new Point(txtSendMonitorPath.Right + 18, row1Top - 2);
        btnSendBrowseMonitor.Size = new Size(browseWidth, 27);

        lblSendAddress.Location = new Point(labelLeft, row2Top + 2);
        txtSendAddress.Location = new Point(inputLeft, row2Top);
        txtSendAddress.Size = new Size(260, 23);
        lblSendPort.Location = new Point(rightLabelLeft, row2Top + 2);
        numSendPort.Location = new Point(rightInputLeft, row2Top);
        numSendPort.Size = new Size(160, 23);

        lblSendBackupPath.Location = new Point(labelLeft, row3Top + 2);
        txtSendBackupPath.Location = new Point(inputLeft, row3Top);
        txtSendBackupPath.Size = new Size(backupWidth, 23);
        btnSendBrowseBackup.Location = new Point(txtSendBackupPath.Right + 18, row3Top - 2);
        btnSendBrowseBackup.Size = new Size(browseWidth, 27);

        lblSendIdentity.Location = new Point(labelLeft, row4Top + 2);
        txtSendIdentity.Location = new Point(inputLeft, row4Top);
        txtSendIdentity.Size = new Size(260, 23);
        lblSendFilter.Location = new Point(rightLabelLeft, row4Top + 2);
        txtSendFilter.Location = new Point(rightInputLeft, row4Top);
        txtSendFilter.Size = new Size(filterWidth, 23);

        panelSendButtons.Visible = false;
        EnsureSendButtonParent(btnSendAdd);
        EnsureSendButtonParent(btnSendClear);
        EnsureSendButtonParent(btnSendStart);
        EnsureSendButtonParent(btnSendStop);
        EnsureSendButtonParent(btnSendDelete);

        ConfigureActionButtonSize(btnSendAdd, 74);
        ConfigureActionButtonSize(btnSendClear, 74);
        ConfigureActionButtonSize(btnSendStart, 96);
        ConfigureActionButtonSize(btnSendStop, 96);
        ConfigureActionButtonSize(btnSendDelete, 108);

        btnSendAdd.Text = "添加";
        btnSendClear.Text = "清空";
        btnSendStart.Text = "启动发送";
        btnSendStop.Text = "停止发送";
        btnSendDelete.Text = "删除选中";

        btnSendAdd.Location = new Point(labelLeft, buttonRowTop);
        btnSendClear.Location = new Point(btnSendAdd.Right + 12, buttonRowTop);
        btnSendStart.Location = new Point(btnSendClear.Right + 12, buttonRowTop);
        btnSendStop.Location = new Point(btnSendStart.Right + 12, buttonRowTop);
        btnSendDelete.Location = new Point(btnSendStop.Right + 12, buttonRowTop);

        dgvSendConfig.Dock = DockStyle.None;
        dgvSendConfig.Location = new Point(10, 24);
        dgvSendConfig.Size = new Size(
            Math.Max(240, grpSendLinks.ClientSize.Width - 20),
            Math.Max(160, grpSendLinks.ClientSize.Height - 34));
    }

    private void EnsureSendButtonParent(Button button)
    {
        if (button.Parent != grpSendAdd)
        {
            grpSendAdd.Controls.Add(button);
            button.BringToFront();
        }
    }

    private void LayoutSendStatePage()
    {
        var width = Math.Max(900, pageSendState.ClientSize.Width);
        var height = Math.Max(420, pageSendState.ClientSize.Height - 60);

        splitSendState.SetBounds(0, 60, width, height);
        splitSendState.SplitterDistance = Math.Max(180, (height - splitSendState.SplitterWidth) / 2);

        grpSendState.Dock = DockStyle.None;
        grpSendLog.Dock = DockStyle.None;
        grpSendState.SetBounds(0, 0, splitSendState.Panel1.ClientSize.Width, splitSendState.Panel1.ClientSize.Height);
        grpSendLog.SetBounds(0, 0, splitSendState.Panel2.ClientSize.Width, splitSendState.Panel2.ClientSize.Height);

        dgvSendState.Dock = DockStyle.None;
        dgvSendLog.Dock = DockStyle.None;
        dgvSendState.Location = new Point(10, 24);
        dgvSendLog.Location = new Point(10, 24);
        dgvSendState.Size = new Size(
            Math.Max(240, grpSendState.ClientSize.Width - 20),
            Math.Max(120, grpSendState.ClientSize.Height - 34));
        dgvSendLog.Size = new Size(
            Math.Max(240, grpSendLog.ClientSize.Width - 20),
            Math.Max(120, grpSendLog.ClientSize.Height - 34));
    }

    private void LayoutReceiveConfigPage()
    {
        const int left = 0;
        const int top = 60;
        const int margin = 12;
        var width = Math.Max(900, pageReceiveConfig.ClientSize.Width);
        var height = Math.Max(600, pageReceiveConfig.ClientSize.Height);

        grpReceiveListen.SetBounds(left, top, width, 84);
        grpReceiveAdd.SetBounds(left, grpReceiveListen.Bottom + margin, width, 224);
        grpReceiveLinks.SetBounds(left, grpReceiveAdd.Bottom + margin, width, Math.Max(220, height - (grpReceiveAdd.Bottom + margin)));

        LayoutReceiveConfigContent();
    }

    private void LayoutReceiveConfigContent()
    {
        // 接收配置页按发送配置页的布局节奏重新排版：
        // 第一行放监听参数，第二个分组内按“左字段 + 右字段 / 目录整行 / 操作按钮”排列。
        const int labelLeft = 18;
        const int inputLeft = 96;
        const int rightLabelLeft = 388;
        const int rightInputLeft = 466;
        const int row1Top = 30;
        const int row2Top = 76;
        const int row3Top = 122;
        const int buttonRowTop = 166;
        const int browseWidth = 76;

        var listenWidth = Math.Max(900, grpReceiveListen.ClientSize.Width);
        var addWidth = Math.Max(900, grpReceiveAdd.ClientSize.Width);
        var widePathWidth = Math.Max(360, addWidth - inputLeft - browseWidth - 114);

        lblReceiveListenIP.Location = new Point(labelLeft, 32);
        txtReceiveListenIP.Location = new Point(inputLeft, 30);
        txtReceiveListenIP.Size = new Size(220, 23);
        lblReceiveListenPort.Location = new Point(352, 32);
        numReceiveListenPort.Location = new Point(430, 30);
        numReceiveListenPort.Size = new Size(160, 23);

        if (listenWidth > 760)
        {
            numReceiveListenPort.Location = new Point(Math.Min(listenWidth - 220, 430), 30);
        }

        lblReceiveLinkName.Location = new Point(labelLeft, row1Top + 2);
        txtReceiveLinkName.Location = new Point(inputLeft, row1Top);
        txtReceiveLinkName.Size = new Size(260, 23);

        lblReceiveRemoteIP.Location = new Point(rightLabelLeft, row1Top + 2);
        txtReceiveRemoteIP.Location = new Point(rightInputLeft, row1Top);
        txtReceiveRemoteIP.Size = new Size(Math.Max(260, addWidth - rightInputLeft - 120), 23);

        lblReceivePath.Location = new Point(labelLeft, row2Top + 2);
        txtReceivePath.Location = new Point(inputLeft, row2Top);
        txtReceivePath.Size = new Size(widePathWidth, 23);
        btnReceiveBrowsePath.Location = new Point(txtReceivePath.Right + 18, row2Top - 2);
        btnReceiveBrowsePath.Size = new Size(browseWidth, 27);

        lblReceiveIdentity.Location = new Point(labelLeft, row3Top + 2);
        txtReceiveIdentity.Location = new Point(inputLeft, row3Top);
        txtReceiveIdentity.Size = new Size(260, 23);

        // 这里不再依赖 FlowLayoutPanel 自动排布，避免运行时把后面的按钮压成小白块。
        panelReceiveButtons.Visible = false;

        EnsureReceiveButtonParent(btnReceiveAdd);
        EnsureReceiveButtonParent(btnReceiveClear);
        EnsureReceiveButtonParent(btnReceiveStart);
        EnsureReceiveButtonParent(btnReceiveStop);
        EnsureReceiveButtonParent(btnReceiveDelete);

        ConfigureActionButtonSize(btnReceiveAdd, 74);
        ConfigureActionButtonSize(btnReceiveClear, 74);
        ConfigureActionButtonSize(btnReceiveStart, 96);
        ConfigureActionButtonSize(btnReceiveStop, 96);
        ConfigureActionButtonSize(btnReceiveDelete, 108);

        btnReceiveAdd.Text = "添加";
        btnReceiveClear.Text = "清空";
        btnReceiveStart.Text = "启动接收";
        btnReceiveStop.Text = "停止接收";
        btnReceiveDelete.Text = "删除选中";

        btnReceiveAdd.Location = new Point(labelLeft, buttonRowTop);
        btnReceiveClear.Location = new Point(btnReceiveAdd.Right + 12, buttonRowTop);
        btnReceiveStart.Location = new Point(btnReceiveClear.Right + 12, buttonRowTop);
        btnReceiveStop.Location = new Point(btnReceiveStart.Right + 12, buttonRowTop);
        btnReceiveDelete.Location = new Point(btnReceiveStop.Right + 12, buttonRowTop);

        // DataGridView 在部分手工布局场景下不会自动重新填满父容器，这里同步把列表区内容也强制铺开。
        dgvReceiveConfig.Dock = DockStyle.None;
        dgvReceiveConfig.Location = new Point(10, 24);
        dgvReceiveConfig.Size = new Size(
            Math.Max(240, grpReceiveLinks.ClientSize.Width - 20),
            Math.Max(160, grpReceiveLinks.ClientSize.Height - 34));
    }

    private void EnsureReceiveButtonParent(Button button)
    {
        if (button.Parent != grpReceiveAdd)
        {
            grpReceiveAdd.Controls.Add(button);
            button.BringToFront();
        }
    }

    private static void ConfigureActionButtonSize(Button button, int width)
    {
        button.AutoSize = false;
        button.Margin = new Padding(0, 0, 10, 0);
        button.Padding = new Padding(0);
        button.Size = new Size(width, 34);
    }

    private void LayoutReceiveStatePage()
    {
        var width = Math.Max(900, pageReceiveState.ClientSize.Width);
        var height = Math.Max(420, pageReceiveState.ClientSize.Height - 60);

        splitReceiveState.SetBounds(0, 60, width, height);
        splitReceiveState.SplitterDistance = Math.Max(180, (height - splitReceiveState.SplitterWidth) / 2);

        grpReceiveState.Dock = DockStyle.None;
        grpReceiveHistory.Dock = DockStyle.None;
        grpReceiveState.SetBounds(0, 0, splitReceiveState.Panel1.ClientSize.Width, splitReceiveState.Panel1.ClientSize.Height);
        grpReceiveHistory.SetBounds(0, 0, splitReceiveState.Panel2.ClientSize.Width, splitReceiveState.Panel2.ClientSize.Height);

        dgvReceiveState.Dock = DockStyle.None;
        dgvReceiveHistory.Dock = DockStyle.None;
        dgvReceiveState.Location = new Point(10, 24);
        dgvReceiveHistory.Location = new Point(10, 24);
        dgvReceiveState.Size = new Size(
            Math.Max(240, grpReceiveState.ClientSize.Width - 20),
            Math.Max(120, grpReceiveState.ClientSize.Height - 34));
        dgvReceiveHistory.Size = new Size(
            Math.Max(240, grpReceiveHistory.ClientSize.Width - 20),
            Math.Max(120, grpReceiveHistory.ClientSize.Height - 34));
    }

    private void LayoutAllPage()
    {
        const int top = 60;
        const int gap = 12;
        var width = Math.Max(980, pageAll.ClientSize.Width);
        var height = Math.Max(520, pageAll.ClientSize.Height - top);

        // “全部”页改成手工总览布局，避免 TableLayoutPanel 在多次切换后塌成左上角一小块。
        tableAll.Visible = false;

        EnsureAllPageParent(grpAllSummary);
        EnsureAllPageParent(grpAllSendLinks);
        EnsureAllPageParent(grpAllReceiveLinks);
        EnsureAllPageParent(grpAllActive);
        EnsureAllPageParent(grpAllHistory);
        EnsureAllPageParent(grpAllLog);

        var leftWidth = (width - gap) / 2;
        var rightWidth = width - leftWidth - gap;
        var summaryHeight = 78;
        var contentTop = top + summaryHeight + gap;
        var bottomHeight = 180;
        var middleHeight = Math.Max(170, (height - summaryHeight - bottomHeight - gap * 3) / 2);

        grpAllSummary.SetBounds(0, top, width, summaryHeight);
        grpAllSendLinks.SetBounds(0, contentTop, leftWidth, middleHeight);
        grpAllActive.SetBounds(leftWidth + gap, contentTop, rightWidth, middleHeight);
        grpAllReceiveLinks.SetBounds(0, grpAllSendLinks.Bottom + gap, leftWidth, middleHeight);
        grpAllHistory.SetBounds(leftWidth + gap, grpAllActive.Bottom + gap, rightWidth, middleHeight);
        grpAllLog.SetBounds(0, Math.Max(grpAllReceiveLinks.Bottom, grpAllHistory.Bottom) + gap, width, bottomHeight);

        panelSummary.Dock = DockStyle.None;
        panelSummary.Location = new Point(12, 26);
        panelSummary.Size = new Size(grpAllSummary.ClientSize.Width - 24, grpAllSummary.ClientSize.Height - 36);
        panelSummary.Visible = false;

        EnsureSummaryLabelParent(lblAllSendCount);
        EnsureSummaryLabelParent(lblAllReceiveCount);
        EnsureSummaryLabelParent(lblAllActiveCount);
        EnsureSummaryLabelParent(lblAllHistoryCount);

        var cardGap = 12;
        var cardWidth = Math.Max(180, (grpAllSummary.ClientSize.Width - 20 - cardGap * 3) / 4);
        var cardTop = 30;
        var cardHeight = 34;

        LayoutSummaryLabel(lblAllSendCount, 10, cardTop, cardWidth, cardHeight);
        LayoutSummaryLabel(lblAllReceiveCount, lblAllSendCount.Right + cardGap, cardTop, cardWidth, cardHeight);
        LayoutSummaryLabel(lblAllActiveCount, lblAllReceiveCount.Right + cardGap, cardTop, cardWidth, cardHeight);
        LayoutSummaryLabel(lblAllHistoryCount, lblAllActiveCount.Right + cardGap, cardTop, cardWidth, cardHeight);

        LayoutOverviewGrid(dgvAllSendLinks, grpAllSendLinks);
        LayoutOverviewGrid(dgvAllReceiveLinks, grpAllReceiveLinks);
        LayoutOverviewGrid(dgvAllActive, grpAllActive);
        LayoutOverviewGrid(dgvAllHistory, grpAllHistory);
        LayoutOverviewGrid(dgvAllLog, grpAllLog);
    }

    private void EnsureAllPageParent(Control control)
    {
        if (control.Parent != pageAll)
        {
            pageAll.Controls.Add(control);
            control.BringToFront();
        }
    }

    private void EnsureSummaryLabelParent(Label label)
    {
        if (label.Parent != grpAllSummary)
        {
            grpAllSummary.Controls.Add(label);
            label.BringToFront();
        }
    }

    private static void LayoutSummaryLabel(Label label, int x, int y, int width, int height)
    {
        label.AutoSize = false;
        label.Location = new Point(x, y);
        label.Margin = new Padding(0);
        label.Size = new Size(width, height);
        label.Padding = new Padding(12, 0, 0, 0);
        label.BackColor = Color.FromArgb(244, 247, 252);
        label.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static void LayoutOverviewGrid(DataGridView grid, GroupBox group)
    {
        grid.Dock = DockStyle.None;
        grid.Location = new Point(10, 24);
        grid.Size = new Size(
            Math.Max(240, group.ClientSize.Width - 20),
            Math.Max(120, group.ClientSize.Height - 34));
    }

    private void ShowPage(string key)
    {
        foreach (var pair in _pages)
        {
            pair.Value.Visible = string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase);
            pair.Value.Dock = DockStyle.Fill;
        }

        foreach (var pair in _navButtons)
        {
            var selected = string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase);
            pair.Value.BackColor = selected ? Color.FromArgb(224, 236, 255) : Color.White;
            pair.Value.FlatAppearance.BorderColor = selected ? Color.FromArgb(60, 120, 216) : Color.FromArgb(205, 210, 220);
        }

        // 页面切换后，刚显示出来的页才有正确的 ClientSize，这里立刻重排，避免控件停留在设计器的左上角小尺寸。
        ApplyResponsiveLayout();
        if (IsHandleCreated)
        {
            BeginInvoke(new Action(ApplyResponsiveLayout));
        }
    }

    private void LoadConfig()
    {
        try
        {
            var document = LinkConfigService.Load(_configPath);

            _sendLinks.Clear();
            foreach (var item in document.SendLinks)
            {
                _sendLinks.Add(item);
            }

            _receiveLinks.Clear();
            foreach (var item in document.ReceiveLinks)
            {
                _receiveLinks.Add(item);
            }

            txtReceiveListenIP.Text = document.ReceiveIP;
            numReceiveListenPort.Value = Math.Clamp(document.ReceivePort, 1, 65535);

            UpdateOverviewCounts();
            RefreshAllGrids();
            AddLog($"已加载配置: {_configPath}");
        }
        catch (Exception ex)
        {
            AddLog($"加载配置失败: {ex.Message}");
        }
    }

    private void SaveConfig()
    {
        try
        {
            var document = new LinkConfigDocument
            {
                ReceiveIP = txtReceiveListenIP.Text.Trim(),
                ReceivePort = (int)numReceiveListenPort.Value
            };

            foreach (var item in _sendLinks)
            {
                document.SendLinks.Add(item);
            }

            foreach (var item in _receiveLinks)
            {
                document.ReceiveLinks.Add(item);
            }

            LinkConfigService.Save(_configPath, document);
            UpdateOverviewCounts();
            AddLog($"已保存配置: {_configPath}");
        }
        catch (Exception ex)
        {
            AddLog($"保存配置失败: {ex.Message}");
        }
    }

    private void AddSendLink()
    {
        if (string.IsNullOrWhiteSpace(txtSendLinkName.Text) ||
            string.IsNullOrWhiteSpace(txtSendMonitorPath.Text) ||
            string.IsNullOrWhiteSpace(txtSendAddress.Text) ||
            string.IsNullOrWhiteSpace(txtSendIdentity.Text))
        {
            MessageBox.Show("请完整填写发送链路的必填项。", "提示");
            return;
        }

        _sendLinks.Add(new SendLinkConfig
        {
            LinkName = txtSendLinkName.Text.Trim(),
            MonitorPath = txtSendMonitorPath.Text.Trim(),
            RemoteAddress = txtSendAddress.Text.Trim(),
            RemotePort = (int)numSendPort.Value,
            BackupPath = txtSendBackupPath.Text.Trim(),
            LinkIdentity = txtSendIdentity.Text.Trim(),
            FilterType = txtSendFilter.Text.Trim()
        });

        ClearSendInputs();
        UpdateOverviewCounts();
        RefreshAllGrids();
    }

    private void AddReceiveLink()
    {
        if (string.IsNullOrWhiteSpace(txtReceiveLinkName.Text) ||
            string.IsNullOrWhiteSpace(txtReceivePath.Text) ||
            string.IsNullOrWhiteSpace(txtReceiveIdentity.Text))
        {
            MessageBox.Show("请完整填写接收链路的必填项。", "提示");
            return;
        }

        _receiveLinks.Add(new ReceiveLinkConfig
        {
            LinkName = txtReceiveLinkName.Text.Trim(),
            RemoteIP = txtReceiveRemoteIP.Text.Trim(),
            ReceivePath = txtReceivePath.Text.Trim(),
            LinkIdentity = txtReceiveIdentity.Text.Trim()
        });

        ClearReceiveInputs();
        UpdateOverviewCounts();
        RefreshAllGrids();
    }

    private void DeleteSelectedSend()
    {
        if (dgvSendConfig.CurrentRow?.DataBoundItem is SendLinkConfig item)
        {
            _sendLinks.Remove(item);
            UpdateOverviewCounts();
            RefreshAllGrids();
        }
    }

    private void DeleteSelectedReceive()
    {
        if (dgvReceiveConfig.CurrentRow?.DataBoundItem is ReceiveLinkConfig item)
        {
            _receiveLinks.Remove(item);
            UpdateOverviewCounts();
            RefreshAllGrids();
        }
    }

    private void StartSend()
    {
        SaveConfig();
        _runtime.StartSend(_sendLinks);
        AddLog("发送链路已启动。");
    }

    private void StartReceive()
    {
        SaveConfig();
        _runtime.StartReceive(txtReceiveListenIP.Text.Trim(), (int)numReceiveListenPort.Value, _receiveLinks);
        AddLog("接收链路已启动。");
    }

    private void StartAll()
    {
        SaveConfig();
        _runtime.StartReceive(txtReceiveListenIP.Text.Trim(), (int)numReceiveListenPort.Value, _receiveLinks);
        _runtime.StartSend(_sendLinks);
        AddLog("本机收发链路已全部启动。");
    }

    private void StopAll()
    {
        _runtime.StopSend();
        _runtime.StopReceive();
        AddLog("本机收发链路已全部停止。");
    }

    private void ClearSendInputs()
    {
        txtSendLinkName.Clear();
        txtSendMonitorPath.Clear();
        txtSendAddress.Clear();
        numSendPort.Value = 12092;
        txtSendBackupPath.Clear();
        txtSendIdentity.Clear();
        txtSendFilter.Clear();
    }

    private void ClearReceiveInputs()
    {
        txtReceiveLinkName.Clear();
        txtReceiveRemoteIP.Clear();
        txtReceivePath.Clear();
        txtReceiveIdentity.Clear();
    }

    private static void BrowseFolder(TextBox target)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            target.Text = dialog.SelectedPath;
        }
    }

    private void HandleTransferUpdated(TransferSnapshot snapshot)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<TransferSnapshot>(HandleTransferUpdated), snapshot);
            return;
        }

        // 实时任务先进入进行中列表，结束后转入最近结果。
        if (!_activeIndex.TryGetValue(snapshot.Id, out var row))
        {
            row = new TransferRow(snapshot);
            _activeIndex[snapshot.Id] = row;
            _activeTransfers.Insert(0, row);
            UpdateOverviewCounts();
        }

        row.Apply(snapshot);
        RefreshGrid(dgvSendState);
        RefreshGrid(dgvReceiveState);
        RefreshGrid(dgvAllActive);
        ApplyTransferFilters();

        if (snapshot.Stage is TransferStage.Succeeded or TransferStage.Failed)
        {
            _activeTransfers.Remove(row);
            _activeIndex.Remove(snapshot.Id);
            _historyTransfers.Insert(0, new TransferRow(snapshot));
            while (_historyTransfers.Count > 200)
            {
                _historyTransfers.RemoveAt(_historyTransfers.Count - 1);
            }

            RefreshGrid(dgvSendState);
            RefreshGrid(dgvReceiveState);
            RefreshGrid(dgvAllActive);
            RefreshGrid(dgvReceiveHistory);
            RefreshGrid(dgvAllHistory);
            ApplyTransferFilters();
            UpdateOverviewCounts();
        }
    }

    private void AddLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(AddLog), message);
            return;
        }

        _logRows.Insert(0, new LogRow
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Message = message
        });

        while (_logRows.Count > 300)
        {
            _logRows.RemoveAt(_logRows.Count - 1);
        }

        RefreshGrid(dgvSendLog);
        RefreshGrid(dgvAllLog);
    }

    private void UpdateOverviewCounts()
    {
        var sendActive = _activeTransfers.Count(x => string.Equals(x.Direction, "发送", StringComparison.Ordinal));
        var receiveActive = _activeTransfers.Count(x => string.Equals(x.Direction, "接收", StringComparison.Ordinal));
        var sendHistory = _historyTransfers.Count(x => string.Equals(x.Direction, "发送", StringComparison.Ordinal));
        var receiveHistory = _historyTransfers.Count(x => string.Equals(x.Direction, "接收", StringComparison.Ordinal));

        lblAllSendCount.Text = $"发送链路: {_sendLinks.Count}";
        lblAllReceiveCount.Text = $"接收链路: {_receiveLinks.Count}";
        lblAllActiveCount.Text = $"实时任务: {_activeTransfers.Count}  发:{sendActive} / 收:{receiveActive}";
        lblAllHistoryCount.Text = $"最近结果: {_historyTransfers.Count}  发:{sendHistory} / 收:{receiveHistory}";
    }

    private void RefreshAllGrids()
    {
        RefreshGrid(dgvSendConfig);
        RefreshGrid(dgvAllSendLinks);
        RefreshGrid(dgvReceiveConfig);
        RefreshGrid(dgvAllReceiveLinks);
        RefreshGrid(dgvSendState);
        RefreshGrid(dgvReceiveState);
        RefreshGrid(dgvAllActive);
        RefreshGrid(dgvReceiveHistory);
        RefreshGrid(dgvAllHistory);
        RefreshGrid(dgvSendLog);
        RefreshGrid(dgvAllLog);
        ApplyTransferFilters();
    }

    private static void RefreshGrid(DataGridView grid)
    {
        if (grid.DataSource is null)
        {
            return;
        }

        var manager = grid.BindingContext?[grid.DataSource] as CurrencyManager;
        manager?.Refresh();
    }

    private void BindSendGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.DataSource = _sendLinks;
        grid.Columns.Clear();
        grid.Columns.Add(CreateTextColumn("链路名称", nameof(SendLinkConfig.LinkName), 90));
        grid.Columns.Add(CreateTextColumn("监视目录", nameof(SendLinkConfig.MonitorPath), 220));
        grid.Columns.Add(CreateTextColumn("发送地址", nameof(SendLinkConfig.RemoteAddress), 100));
        grid.Columns.Add(CreateTextColumn("端口", nameof(SendLinkConfig.RemotePort), 70));
        grid.Columns.Add(CreateTextColumn("链路标识", nameof(SendLinkConfig.LinkIdentity), 80));
        grid.Columns.Add(CreateTextColumn("过滤类型", nameof(SendLinkConfig.FilterType), 100));
    }

    private void BindReceiveGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.DataSource = _receiveLinks;
        grid.Columns.Clear();
        grid.Columns.Add(CreateTextColumn("链路名称", nameof(ReceiveLinkConfig.LinkName), 90));
        grid.Columns.Add(CreateTextColumn("远端 IP", nameof(ReceiveLinkConfig.RemoteIP), 110));
        grid.Columns.Add(CreateTextColumn("接收目录", nameof(ReceiveLinkConfig.ReceivePath), 240));
        grid.Columns.Add(CreateTextColumn("链路标识", nameof(ReceiveLinkConfig.LinkIdentity), 90));
    }

    private void BindTransferGrid(DataGridView grid, bool sendOnly)
    {
        grid.AutoGenerateColumns = false;
        grid.DataSource = _activeTransfers;
        grid.Columns.Clear();
        grid.Columns.Add(CreateTextColumn("方向", nameof(TransferRow.Direction), 60));
        grid.Columns.Add(CreateTextColumn("链路", nameof(TransferRow.LinkName), 90));
        grid.Columns.Add(CreateTextColumn("文件名", nameof(TransferRow.FileName), 180));
        grid.Columns.Add(CreateTextColumn("目标", nameof(TransferRow.Target), 120));
        grid.Columns.Add(CreateTextColumn("进度", nameof(TransferRow.ProgressText), 120));
        grid.Columns.Add(CreateTextColumn("状态", nameof(TransferRow.Stage), 70));
        grid.Columns.Add(CreateTextColumn("说明", nameof(TransferRow.Message), 180));
        grid.Tag = sendOnly ? "发送" : "接收";
        grid.DataBindingComplete -= Grid_DataBindingComplete;
        grid.DataBindingComplete += Grid_DataBindingComplete;
    }

    private void BindCombinedTransferGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.DataSource = _activeTransfers;
        grid.Columns.Clear();
        grid.Columns.Add(CreateTextColumn("方向", nameof(TransferRow.Direction), 60));
        grid.Columns.Add(CreateTextColumn("链路", nameof(TransferRow.LinkName), 90));
        grid.Columns.Add(CreateTextColumn("文件名", nameof(TransferRow.FileName), 180));
        grid.Columns.Add(CreateTextColumn("目标", nameof(TransferRow.Target), 120));
        grid.Columns.Add(CreateTextColumn("进度", nameof(TransferRow.ProgressText), 120));
        grid.Columns.Add(CreateTextColumn("状态", nameof(TransferRow.Stage), 70));
        grid.Columns.Add(CreateTextColumn("说明", nameof(TransferRow.Message), 180));
    }

    private void BindHistoryGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.DataSource = _historyTransfers;
        grid.Columns.Clear();
        grid.Columns.Add(CreateTextColumn("方向", nameof(TransferRow.Direction), 60));
        grid.Columns.Add(CreateTextColumn("链路", nameof(TransferRow.LinkName), 90));
        grid.Columns.Add(CreateTextColumn("文件名", nameof(TransferRow.FileName), 180));
        grid.Columns.Add(CreateTextColumn("结果", nameof(TransferRow.Stage), 70));
        grid.Columns.Add(CreateTextColumn("进度", nameof(TransferRow.ProgressText), 120));
        grid.Columns.Add(CreateTextColumn("说明", nameof(TransferRow.Message), 180));
        grid.Columns.Add(CreateTextColumn("完成时间", nameof(TransferRow.TimestampText), 130));
        grid.Tag = ReferenceEquals(grid, dgvReceiveHistory) ? "接收" : null;
        grid.DataBindingComplete -= Grid_DataBindingComplete;
        grid.DataBindingComplete += Grid_DataBindingComplete;
    }

    private void BindLogGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.DataSource = _logRows;
        grid.Columns.Clear();
        grid.Columns.Add(CreateTextColumn("时间", nameof(LogRow.Timestamp), 140));
        grid.Columns.Add(CreateTextColumn("内容", nameof(LogRow.Message), 700));
    }

    private static DataGridViewTextBoxColumn CreateTextColumn(string header, string propertyName, int minWidth)
    {
        return new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = propertyName,
            MinimumWidth = minWidth
        };
    }

    private void Grid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (sender is DataGridView grid)
        {
            ApplyGridDirectionFilter(grid);
        }
    }

    private void ApplyTransferFilters()
    {
        ApplyGridDirectionFilter(dgvSendState);
        ApplyGridDirectionFilter(dgvReceiveState);
        ApplyGridDirectionFilter(dgvReceiveHistory);
        ApplyGridDirectionFilter(dgvAllActive);
        ApplyGridDirectionFilter(dgvAllHistory);
    }

    private static void ApplyGridDirectionFilter(DataGridView grid)
    {
        var expectedDirection = grid.Tag as string;
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.DataBoundItem is not TransferRow transferRow)
            {
                continue;
            }

            row.Visible = string.IsNullOrWhiteSpace(expectedDirection) ||
                string.Equals(transferRow.Direction, expectedDirection, StringComparison.Ordinal);
        }
    }

    private sealed class TransferRow
    {
        public TransferRow(TransferSnapshot snapshot)
        {
            Apply(snapshot);
        }

        public string Direction { get; private set; } = string.Empty;
        public string LinkName { get; private set; } = string.Empty;
        public string FileName { get; private set; } = string.Empty;
        public string Target { get; private set; } = string.Empty;
        public string ProgressText { get; private set; } = string.Empty;
        public string Stage { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public string TimestampText { get; private set; } = string.Empty;

        public void Apply(TransferSnapshot snapshot)
        {
            Direction = snapshot.Direction == TransferDirection.Send ? "发送" : "接收";
            LinkName = snapshot.LinkName;
            FileName = snapshot.FileName;
            Target = snapshot.Target;
            ProgressText = snapshot.TotalBytes <= 0
                ? "-"
                : $"{snapshot.BytesTransferred / 1024d / 1024d:F1} / {snapshot.TotalBytes / 1024d / 1024d:F1} MB";
            Stage = snapshot.Stage switch
            {
                TransferStage.Queued => "排队中",
                TransferStage.Running => "传输中",
                TransferStage.Succeeded => "成功",
                _ => "失败"
            };
            Message = snapshot.Message;
            TimestampText = snapshot.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    private sealed class LogRow
    {
        public string Timestamp { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}
