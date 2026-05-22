namespace FileTransferTool.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private MenuStrip menuStripMain = null!;
    private ToolStripMenuItem menuConfig = null!;
    private ToolStripMenuItem btnMenuSave = null!;
    private ToolStripMenuItem btnMenuLoad = null!;
    private ToolStripMenuItem menuHelp = null!;
    private ToolStripMenuItem btnMenuStartAll = null!;
    private ToolStripMenuItem btnMenuStopAll = null!;
    private SplitContainer splitMain = null!;
    private Panel panelSidebar = null!;
    private Label lblSidebarTitle = null!;
    private FlowLayoutPanel panelNavButtons = null!;
    private Button btnNavSendConfig = null!;
    private Button btnNavSendState = null!;
    private Button btnNavReceiveConfig = null!;
    private Button btnNavReceiveState = null!;
    private Button btnNavAll = null!;
    private Panel panelContent = null!;
    private Panel pageSendConfig = null!;
    private Panel pageSendState = null!;
    private Panel pageReceiveConfig = null!;
    private Panel pageReceiveState = null!;
    private Panel pageAll = null!;
    private Label lblSendConfigTitle = null!;
    private Label lblSendConfigDesc = null!;
    private GroupBox grpSendAdd = null!;
    private GroupBox grpSendLinks = null!;
    private Label lblSendLinkName = null!;
    private TextBox txtSendLinkName = null!;
    private Label lblSendMonitorPath = null!;
    private TextBox txtSendMonitorPath = null!;
    private Button btnSendBrowseMonitor = null!;
    private Label lblSendAddress = null!;
    private TextBox txtSendAddress = null!;
    private Label lblSendPort = null!;
    private NumericUpDown numSendPort = null!;
    private Label lblSendBackupPath = null!;
    private TextBox txtSendBackupPath = null!;
    private Button btnSendBrowseBackup = null!;
    private Label lblSendIdentity = null!;
    private TextBox txtSendIdentity = null!;
    private Label lblSendFilter = null!;
    private TextBox txtSendFilter = null!;
    private FlowLayoutPanel panelSendButtons = null!;
    private Button btnSendAdd = null!;
    private Button btnSendClear = null!;
    private Button btnSendStart = null!;
    private Button btnSendStop = null!;
    private Button btnSendDelete = null!;
    private DataGridView dgvSendConfig = null!;
    private DataGridView dgvSendState = null!;
    private DataGridView dgvSendLog = null!;
    private GroupBox grpSendState = null!;
    private GroupBox grpSendLog = null!;
    private SplitContainer splitSendState = null!;
    private Label lblSendStateTitle = null!;
    private Label lblSendStateDesc = null!;
    private GroupBox grpReceiveListen = null!;
    private Label lblReceiveConfigTitle = null!;
    private Label lblReceiveConfigDesc = null!;
    private Label lblReceiveListenIP = null!;
    private TextBox txtReceiveListenIP = null!;
    private Label lblReceiveListenPort = null!;
    private NumericUpDown numReceiveListenPort = null!;
    private GroupBox grpReceiveAdd = null!;
    private Label lblReceiveLinkName = null!;
    private TextBox txtReceiveLinkName = null!;
    private Label lblReceiveRemoteIP = null!;
    private TextBox txtReceiveRemoteIP = null!;
    private Label lblReceivePath = null!;
    private TextBox txtReceivePath = null!;
    private Button btnReceiveBrowsePath = null!;
    private Label lblReceiveIdentity = null!;
    private TextBox txtReceiveIdentity = null!;
    private FlowLayoutPanel panelReceiveButtons = null!;
    private Button btnReceiveAdd = null!;
    private Button btnReceiveClear = null!;
    private Button btnReceiveStart = null!;
    private Button btnReceiveStop = null!;
    private Button btnReceiveDelete = null!;
    private GroupBox grpReceiveLinks = null!;
    private DataGridView dgvReceiveConfig = null!;
    private Label lblReceiveStateTitle = null!;
    private Label lblReceiveStateDesc = null!;
    private SplitContainer splitReceiveState = null!;
    private GroupBox grpReceiveState = null!;
    private DataGridView dgvReceiveState = null!;
    private GroupBox grpReceiveHistory = null!;
    private DataGridView dgvReceiveHistory = null!;
    private Label lblAllTitle = null!;
    private Label lblAllDesc = null!;
    private TableLayoutPanel tableAll = null!;
    private GroupBox grpAllSummary = null!;
    private FlowLayoutPanel panelSummary = null!;
    private Label lblAllSendCount = null!;
    private Label lblAllReceiveCount = null!;
    private Label lblAllActiveCount = null!;
    private Label lblAllHistoryCount = null!;
    private GroupBox grpAllSendLinks = null!;
    private DataGridView dgvAllSendLinks = null!;
    private GroupBox grpAllReceiveLinks = null!;
    private DataGridView dgvAllReceiveLinks = null!;
    private GroupBox grpAllActive = null!;
    private DataGridView dgvAllActive = null!;
    private GroupBox grpAllHistory = null!;
    private DataGridView dgvAllHistory = null!;
    private GroupBox grpAllLog = null!;
    private DataGridView dgvAllLog = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        menuStripMain = new MenuStrip();
        menuConfig = new ToolStripMenuItem();
        btnMenuSave = new ToolStripMenuItem();
        btnMenuLoad = new ToolStripMenuItem();
        menuHelp = new ToolStripMenuItem();
        btnMenuStartAll = new ToolStripMenuItem();
        btnMenuStopAll = new ToolStripMenuItem();
        splitMain = new SplitContainer();
        panelSidebar = new Panel();
        lblSidebarTitle = new Label();
        panelNavButtons = new FlowLayoutPanel();
        btnNavSendConfig = CreateNavButton("发送配置");
        btnNavSendState = CreateNavButton("发送状态");
        btnNavReceiveConfig = CreateNavButton("接收配置");
        btnNavReceiveState = CreateNavButton("接收状态");
        btnNavAll = CreateNavButton("全部");
        panelContent = new Panel();
        pageSendConfig = new Panel();
        lblSendConfigDesc = new Label();
        lblSendConfigTitle = new Label();
        grpSendAdd = new GroupBox();
        lblSendLinkName = new Label();
        txtSendLinkName = new TextBox();
        lblSendMonitorPath = new Label();
        txtSendMonitorPath = new TextBox();
        btnSendBrowseMonitor = CreateActionButton("浏览");
        lblSendAddress = new Label();
        txtSendAddress = new TextBox();
        lblSendPort = new Label();
        numSendPort = new NumericUpDown();
        lblSendBackupPath = new Label();
        txtSendBackupPath = new TextBox();
        btnSendBrowseBackup = CreateActionButton("浏览");
        lblSendIdentity = new Label();
        txtSendIdentity = new TextBox();
        lblSendFilter = new Label();
        txtSendFilter = new TextBox();
        panelSendButtons = new FlowLayoutPanel();
        btnSendAdd = CreateActionButton("添加");
        btnSendClear = CreateActionButton("清空");
        btnSendStart = CreateActionButton("启动发送");
        btnSendStop = CreateActionButton("停止发送");
        btnSendDelete = CreateActionButton("删除选中");
        grpSendLinks = new GroupBox();
        dgvSendConfig = CreateGridView();
        pageSendState = new Panel();
        splitSendState = new SplitContainer();
        grpSendState = new GroupBox();
        dgvSendState = CreateGridView();
        grpSendLog = new GroupBox();
        dgvSendLog = CreateGridView();
        lblSendStateDesc = new Label();
        lblSendStateTitle = new Label();
        pageReceiveConfig = new Panel();
        lblReceiveConfigTitle = new Label();
        lblReceiveConfigDesc = new Label();
        grpReceiveListen = new GroupBox();
        lblReceiveListenIP = new Label();
        txtReceiveListenIP = new TextBox();
        lblReceiveListenPort = new Label();
        numReceiveListenPort = new NumericUpDown();
        grpReceiveAdd = new GroupBox();
        lblReceiveLinkName = new Label();
        txtReceiveLinkName = new TextBox();
        lblReceiveRemoteIP = new Label();
        txtReceiveRemoteIP = new TextBox();
        lblReceivePath = new Label();
        txtReceivePath = new TextBox();
        btnReceiveBrowsePath = CreateActionButton("浏览");
        lblReceiveIdentity = new Label();
        txtReceiveIdentity = new TextBox();
        panelReceiveButtons = new FlowLayoutPanel();
        btnReceiveAdd = CreateActionButton("添加");
        btnReceiveClear = CreateActionButton("清空");
        btnReceiveStart = CreateActionButton("启动接收");
        btnReceiveStop = CreateActionButton("停止接收");
        btnReceiveDelete = CreateActionButton("删除选中");
        grpReceiveLinks = new GroupBox();
        dgvReceiveConfig = CreateGridView();
        pageReceiveState = new Panel();
        splitReceiveState = new SplitContainer();
        grpReceiveState = new GroupBox();
        dgvReceiveState = CreateGridView();
        grpReceiveHistory = new GroupBox();
        dgvReceiveHistory = CreateGridView();
        lblReceiveStateDesc = new Label();
        lblReceiveStateTitle = new Label();
        pageAll = new Panel();
        lblAllTitle = new Label();
        lblAllDesc = new Label();
        tableAll = new TableLayoutPanel();
        grpAllSummary = new GroupBox();
        panelSummary = new FlowLayoutPanel();
        lblAllSendCount = CreateSummaryLabel("发送链路: 0");
        lblAllReceiveCount = CreateSummaryLabel("接收链路: 0");
        lblAllActiveCount = CreateSummaryLabel("实时任务: 0");
        lblAllHistoryCount = CreateSummaryLabel("最近结果: 0");
        grpAllSendLinks = new GroupBox();
        dgvAllSendLinks = CreateGridView();
        grpAllReceiveLinks = new GroupBox();
        dgvAllReceiveLinks = CreateGridView();
        grpAllActive = new GroupBox();
        dgvAllActive = CreateGridView();
        grpAllHistory = new GroupBox();
        dgvAllHistory = CreateGridView();
        grpAllLog = new GroupBox();
        dgvAllLog = CreateGridView();
        menuStripMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        panelSidebar.SuspendLayout();
        panelNavButtons.SuspendLayout();
        panelContent.SuspendLayout();
        pageSendConfig.SuspendLayout();
        grpSendAdd.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numSendPort).BeginInit();
        panelSendButtons.SuspendLayout();
        grpSendLinks.SuspendLayout();
        pageSendState.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitSendState).BeginInit();
        splitSendState.Panel1.SuspendLayout();
        splitSendState.Panel2.SuspendLayout();
        splitSendState.SuspendLayout();
        grpSendState.SuspendLayout();
        grpSendLog.SuspendLayout();
        pageReceiveConfig.SuspendLayout();
        grpReceiveListen.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numReceiveListenPort).BeginInit();
        grpReceiveAdd.SuspendLayout();
        panelReceiveButtons.SuspendLayout();
        grpReceiveLinks.SuspendLayout();
        pageReceiveState.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitReceiveState).BeginInit();
        splitReceiveState.Panel1.SuspendLayout();
        splitReceiveState.Panel2.SuspendLayout();
        splitReceiveState.SuspendLayout();
        grpReceiveState.SuspendLayout();
        grpReceiveHistory.SuspendLayout();
        pageAll.SuspendLayout();
        tableAll.SuspendLayout();
        grpAllSummary.SuspendLayout();
        panelSummary.SuspendLayout();
        grpAllSendLinks.SuspendLayout();
        grpAllReceiveLinks.SuspendLayout();
        grpAllActive.SuspendLayout();
        grpAllHistory.SuspendLayout();
        grpAllLog.SuspendLayout();
        SuspendLayout();
        // top-level
        menuStripMain.Items.AddRange(new ToolStripItem[] { menuConfig, menuHelp });
        menuStripMain.Location = new Point(0, 0);
        menuStripMain.Size = new Size(1380, 25);
        menuConfig.DropDownItems.AddRange(new ToolStripItem[] { btnMenuSave, btnMenuLoad });
        menuConfig.Text = "配置";
        btnMenuSave.Text = "保存链路信息";
        btnMenuLoad.Text = "加载链路信息";
        menuHelp.DropDownItems.AddRange(new ToolStripItem[] { btnMenuStartAll, btnMenuStopAll });
        menuHelp.Text = "帮助";
        btnMenuStartAll.Text = "全部启动";
        btnMenuStopAll.Text = "全部停止";
        splitMain.Dock = DockStyle.Fill;
        splitMain.FixedPanel = FixedPanel.Panel1;
        splitMain.IsSplitterFixed = true;
        splitMain.Location = new Point(0, 25);
        splitMain.Size = new Size(1380, 836);
        splitMain.SplitterDistance = 140;
        splitMain.Panel1.Controls.Add(panelSidebar);
        splitMain.Panel2.Controls.Add(panelContent);
        panelSidebar.BackColor = Color.White;
        panelSidebar.Dock = DockStyle.Fill;
        panelSidebar.Padding = new Padding(10, 16, 10, 12);
        panelSidebar.Controls.Add(panelNavButtons);
        panelSidebar.Controls.Add(lblSidebarTitle);
        lblSidebarTitle.Dock = DockStyle.Top;
        lblSidebarTitle.Height = 26;
        lblSidebarTitle.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
        lblSidebarTitle.Text = "节点导航";
        panelNavButtons.Dock = DockStyle.Fill;
        panelNavButtons.FlowDirection = FlowDirection.TopDown;
        panelNavButtons.WrapContents = false;
        panelNavButtons.Controls.Add(btnNavSendConfig);
        panelNavButtons.Controls.Add(btnNavSendState);
        panelNavButtons.Controls.Add(btnNavReceiveConfig);
        panelNavButtons.Controls.Add(btnNavReceiveState);
        panelNavButtons.Controls.Add(btnNavAll);
        panelContent.BackColor = Color.FromArgb(236, 239, 244);
        panelContent.Dock = DockStyle.Fill;
        panelContent.Padding = new Padding(12);
        panelContent.Controls.Add(pageAll);
        panelContent.Controls.Add(pageReceiveState);
        panelContent.Controls.Add(pageReceiveConfig);
        panelContent.Controls.Add(pageSendState);
        panelContent.Controls.Add(pageSendConfig);
        // send config page
        pageSendConfig.Dock = DockStyle.Fill;
        pageSendConfig.Controls.Add(grpSendLinks);
        pageSendConfig.Controls.Add(grpSendAdd);
        pageSendConfig.Controls.Add(lblSendConfigDesc);
        pageSendConfig.Controls.Add(lblSendConfigTitle);
        lblSendConfigTitle.Dock = DockStyle.Top;
        lblSendConfigTitle.Height = 28;
        lblSendConfigTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        lblSendConfigTitle.Text = "发送配置";
        lblSendConfigDesc.Dock = DockStyle.Top;
        lblSendConfigDesc.Height = 24;
        lblSendConfigDesc.ForeColor = Color.FromArgb(101, 114, 138);
        lblSendConfigDesc.Text = "新增发送链路、配置监视目录和目标服务器。";
        grpSendAdd.Dock = DockStyle.Top;
        grpSendAdd.Height = 240;
        grpSendAdd.Text = "添加链路";
        grpSendLinks.Dock = DockStyle.Fill;
        grpSendLinks.Text = "已配置发送链路";
        grpSendLinks.Controls.Add(dgvSendConfig);
        dgvSendConfig.Dock = DockStyle.Fill;
        ConfigureLabel(lblSendLinkName, "链路名称", 18, 34);
        txtSendLinkName.Location = new Point(96, 36);
        txtSendLinkName.Size = new Size(260, 23);
        ConfigureLabel(lblSendMonitorPath, "监视目录", 388, 34);
        txtSendMonitorPath.Location = new Point(466, 36);
        txtSendMonitorPath.Size = new Size(340, 23);
        btnSendBrowseMonitor.Location = new Point(824, 34);
        btnSendBrowseMonitor.Size = new Size(76, 27);
        ConfigureLabel(lblSendAddress, "发送地址", 18, 74);
        txtSendAddress.Location = new Point(96, 76);
        txtSendAddress.Size = new Size(260, 23);
        ConfigureLabel(lblSendPort, "发送端口", 388, 74);
        numSendPort.Location = new Point(466, 76);
        numSendPort.Minimum = 1;
        numSendPort.Maximum = 65535;
        numSendPort.Value = 12092;
        numSendPort.Size = new Size(160, 23);
        ConfigureLabel(lblSendBackupPath, "备份目录", 18, 114);
        txtSendBackupPath.Location = new Point(96, 116);
        txtSendBackupPath.Size = new Size(710, 23);
        btnSendBrowseBackup.Location = new Point(824, 114);
        btnSendBrowseBackup.Size = new Size(76, 27);
        ConfigureLabel(lblSendIdentity, "链路标识", 18, 154);
        txtSendIdentity.Location = new Point(96, 156);
        txtSendIdentity.Size = new Size(260, 23);
        ConfigureLabel(lblSendFilter, "过滤类型", 388, 154);
        txtSendFilter.Location = new Point(466, 156);
        txtSendFilter.Size = new Size(260, 23);
        panelSendButtons.Location = new Point(18, 190);
        panelSendButtons.Size = new Size(900, 36);
        panelSendButtons.Controls.Add(btnSendAdd);
        panelSendButtons.Controls.Add(btnSendClear);
        panelSendButtons.Controls.Add(btnSendStart);
        panelSendButtons.Controls.Add(btnSendStop);
        panelSendButtons.Controls.Add(btnSendDelete);
        grpSendAdd.Controls.AddRange(new Control[]
        {
            lblSendLinkName, txtSendLinkName, lblSendMonitorPath, txtSendMonitorPath, btnSendBrowseMonitor,
            lblSendAddress, txtSendAddress, lblSendPort, numSendPort, lblSendBackupPath, txtSendBackupPath,
            btnSendBrowseBackup, lblSendIdentity, txtSendIdentity, lblSendFilter, txtSendFilter, panelSendButtons
        });
        // send state
        pageSendState.Dock = DockStyle.Fill;
        pageSendState.Controls.Add(splitSendState);
        pageSendState.Controls.Add(lblSendStateDesc);
        pageSendState.Controls.Add(lblSendStateTitle);
        lblSendStateTitle.Dock = DockStyle.Top;
        lblSendStateTitle.Height = 28;
        lblSendStateTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        lblSendStateTitle.Text = "发送状态";
        lblSendStateDesc.Dock = DockStyle.Top;
        lblSendStateDesc.Height = 24;
        lblSendStateDesc.ForeColor = Color.FromArgb(101, 114, 138);
        lblSendStateDesc.Text = "实时查看发送链路中的文件流转和结果。";
        splitSendState.Dock = DockStyle.Fill;
        splitSendState.Location = new Point(0, 52);
        splitSendState.Orientation = Orientation.Horizontal;
        splitSendState.SplitterDistance = 360;
        splitSendState.Panel1.Controls.Add(grpSendState);
        splitSendState.Panel2.Controls.Add(grpSendLog);
        grpSendState.Dock = DockStyle.Fill;
        grpSendState.Text = "发送中的文件";
        grpSendState.Controls.Add(dgvSendState);
        dgvSendState.Dock = DockStyle.Fill;
        grpSendLog.Dock = DockStyle.Fill;
        grpSendLog.Text = "运行日志";
        grpSendLog.Controls.Add(dgvSendLog);
        dgvSendLog.Dock = DockStyle.Fill;
        // receive config
        pageReceiveConfig.Dock = DockStyle.Fill;
        pageReceiveConfig.Controls.Add(grpReceiveLinks);
        pageReceiveConfig.Controls.Add(grpReceiveAdd);
        pageReceiveConfig.Controls.Add(grpReceiveListen);
        pageReceiveConfig.Controls.Add(lblReceiveConfigDesc);
        pageReceiveConfig.Controls.Add(lblReceiveConfigTitle);
        lblReceiveConfigTitle.Dock = DockStyle.Top;
        lblReceiveConfigTitle.Height = 28;
        lblReceiveConfigTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        lblReceiveConfigTitle.Text = "接收配置";
        lblReceiveConfigDesc.Dock = DockStyle.Top;
        lblReceiveConfigDesc.Height = 24;
        lblReceiveConfigDesc.ForeColor = Color.FromArgb(101, 114, 138);
        lblReceiveConfigDesc.Text = "新增接收链路、配置本机监听地址和落盘目录。";
        grpReceiveListen.Dock = DockStyle.Top;
        grpReceiveListen.Height = 70;
        grpReceiveListen.Text = "本机监听";
        ConfigureLabel(lblReceiveListenIP, "接收 IP", 18, 28);
        txtReceiveListenIP.Location = new Point(96, 30);
        txtReceiveListenIP.Size = new Size(220, 23);
        ConfigureLabel(lblReceiveListenPort, "接收端口", 352, 28);
        numReceiveListenPort.Location = new Point(430, 30);
        numReceiveListenPort.Minimum = 1;
        numReceiveListenPort.Maximum = 65535;
        numReceiveListenPort.Value = 12092;
        numReceiveListenPort.Size = new Size(160, 23);
        grpReceiveListen.Controls.AddRange(new Control[] { lblReceiveListenIP, txtReceiveListenIP, lblReceiveListenPort, numReceiveListenPort });
        grpReceiveAdd.Dock = DockStyle.Top;
        grpReceiveAdd.Height = 230;
        grpReceiveAdd.Text = "添加链路";
        ConfigureLabel(lblReceiveLinkName, "链路名称", 18, 34);
        txtReceiveLinkName.Location = new Point(96, 36);
        txtReceiveLinkName.Size = new Size(260, 23);
        ConfigureLabel(lblReceiveRemoteIP, "远端 IP", 388, 34);
        txtReceiveRemoteIP.Location = new Point(466, 36);
        txtReceiveRemoteIP.Size = new Size(260, 23);
        ConfigureLabel(lblReceivePath, "接收目录", 18, 82);
        txtReceivePath.Location = new Point(96, 84);
        txtReceivePath.Size = new Size(630, 23);
        btnReceiveBrowsePath.Location = new Point(744, 82);
        btnReceiveBrowsePath.Size = new Size(76, 27);
        ConfigureLabel(lblReceiveIdentity, "链路标识", 18, 122);
        txtReceiveIdentity.Location = new Point(96, 124);
        txtReceiveIdentity.Size = new Size(260, 23);
        panelReceiveButtons.Location = new Point(18, 158);
        panelReceiveButtons.Size = new Size(900, 44);
        panelReceiveButtons.Controls.AddRange(new Control[] { btnReceiveAdd, btnReceiveClear, btnReceiveStart, btnReceiveStop, btnReceiveDelete });
        grpReceiveAdd.Controls.AddRange(new Control[]
        {
            lblReceiveLinkName, txtReceiveLinkName, lblReceiveRemoteIP, txtReceiveRemoteIP, lblReceivePath, txtReceivePath,
            btnReceiveBrowsePath, lblReceiveIdentity, txtReceiveIdentity, panelReceiveButtons
        });
        grpReceiveLinks.Dock = DockStyle.Fill;
        grpReceiveLinks.Text = "已配置接收链路";
        grpReceiveLinks.Controls.Add(dgvReceiveConfig);
        dgvReceiveConfig.Dock = DockStyle.Fill;
        // receive state
        pageReceiveState.Dock = DockStyle.Fill;
        pageReceiveState.Controls.Add(splitReceiveState);
        pageReceiveState.Controls.Add(lblReceiveStateDesc);
        pageReceiveState.Controls.Add(lblReceiveStateTitle);
        lblReceiveStateTitle.Dock = DockStyle.Top;
        lblReceiveStateTitle.Height = 28;
        lblReceiveStateTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        lblReceiveStateTitle.Text = "接收状态";
        lblReceiveStateDesc.Dock = DockStyle.Top;
        lblReceiveStateDesc.Height = 24;
        lblReceiveStateDesc.ForeColor = Color.FromArgb(101, 114, 138);
        lblReceiveStateDesc.Text = "实时查看接收链路中的文件流转和结果。";
        splitReceiveState.Dock = DockStyle.Fill;
        splitReceiveState.Location = new Point(0, 52);
        splitReceiveState.Orientation = Orientation.Horizontal;
        splitReceiveState.SplitterDistance = 360;
        splitReceiveState.Panel1.Controls.Add(grpReceiveState);
        splitReceiveState.Panel2.Controls.Add(grpReceiveHistory);
        grpReceiveState.Dock = DockStyle.Fill;
        grpReceiveState.Text = "接收中的文件";
        grpReceiveState.Controls.Add(dgvReceiveState);
        dgvReceiveState.Dock = DockStyle.Fill;
        grpReceiveHistory.Dock = DockStyle.Fill;
        grpReceiveHistory.Text = "最近结果";
        grpReceiveHistory.Controls.Add(dgvReceiveHistory);
        dgvReceiveHistory.Dock = DockStyle.Fill;
        // all page
        pageAll.Dock = DockStyle.Fill;
        pageAll.Controls.Add(tableAll);
        pageAll.Controls.Add(lblAllDesc);
        pageAll.Controls.Add(lblAllTitle);
        lblAllTitle.Dock = DockStyle.Top;
        lblAllTitle.Height = 28;
        lblAllTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        lblAllTitle.Text = "全部";
        lblAllDesc.Dock = DockStyle.Top;
        lblAllDesc.Height = 24;
        lblAllDesc.ForeColor = Color.FromArgb(101, 114, 138);
        lblAllDesc.Text = "统一查看本机发送、接收、实时传输和运行日志。";
        tableAll.Dock = DockStyle.Fill;
        tableAll.ColumnCount = 2;
        tableAll.RowCount = 3;
        tableAll.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        tableAll.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        tableAll.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        tableAll.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        tableAll.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        tableAll.Controls.Add(grpAllSummary, 0, 0);
        tableAll.Controls.Add(grpAllSendLinks, 0, 1);
        tableAll.Controls.Add(grpAllReceiveLinks, 0, 2);
        tableAll.Controls.Add(grpAllActive, 1, 1);
        tableAll.Controls.Add(grpAllHistory, 1, 2);
        tableAll.Controls.Add(grpAllLog, 1, 0);
        grpAllSummary.Dock = DockStyle.Fill;
        grpAllSummary.Text = "概览";
        grpAllSummary.Controls.Add(panelSummary);
        panelSummary.Dock = DockStyle.Fill;
        panelSummary.Controls.AddRange(new Control[] { lblAllSendCount, lblAllReceiveCount, lblAllActiveCount, lblAllHistoryCount });
        grpAllSendLinks.Dock = DockStyle.Fill;
        grpAllSendLinks.Text = "发送链路";
        grpAllSendLinks.Controls.Add(dgvAllSendLinks);
        dgvAllSendLinks.Dock = DockStyle.Fill;
        grpAllReceiveLinks.Dock = DockStyle.Fill;
        grpAllReceiveLinks.Text = "接收链路";
        grpAllReceiveLinks.Controls.Add(dgvAllReceiveLinks);
        dgvAllReceiveLinks.Dock = DockStyle.Fill;
        grpAllActive.Dock = DockStyle.Fill;
        grpAllActive.Text = "实时传输";
        grpAllActive.Controls.Add(dgvAllActive);
        dgvAllActive.Dock = DockStyle.Fill;
        grpAllHistory.Dock = DockStyle.Fill;
        grpAllHistory.Text = "最近结果";
        grpAllHistory.Controls.Add(dgvAllHistory);
        dgvAllHistory.Dock = DockStyle.Fill;
        grpAllLog.Dock = DockStyle.Fill;
        grpAllLog.Text = "运行日志";
        grpAllLog.Controls.Add(dgvAllLog);
        dgvAllLog.Dock = DockStyle.Fill;
        // form
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(236, 239, 244);
        ClientSize = new Size(1380, 861);
        Controls.Add(splitMain);
        Controls.Add(menuStripMain);
        Font = new Font("Microsoft YaHei UI", 9F);
        MainMenuStrip = menuStripMain;
        MinimumSize = new Size(1380, 900);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "文件传输服务";
        menuStripMain.ResumeLayout(false);
        menuStripMain.PerformLayout();
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        panelSidebar.ResumeLayout(false);
        panelNavButtons.ResumeLayout(false);
        panelContent.ResumeLayout(false);
        pageSendConfig.ResumeLayout(false);
        grpSendAdd.ResumeLayout(false);
        grpSendAdd.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numSendPort).EndInit();
        panelSendButtons.ResumeLayout(false);
        panelSendButtons.PerformLayout();
        grpSendLinks.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private static void ConfigureLabel(Label label, string text, int x, int y)
    {
        label.Location = new Point(x, y);
        label.Size = new Size(72, 26);
        label.Text = text;
        label.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static Button CreateNavButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Width = 108,
            Height = 52,
            Margin = new Padding(0, 0, 0, 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(40, 53, 77)
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(205, 210, 220);
        return button;
    }

    private static Button CreateActionButton(string text)
    {
        var button = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(248, 250, 252),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 10, 8),
            Padding = new Padding(14, 6, 14, 6),
            AutoSize = true
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        return button;
    }

    private static Label CreateSummaryLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Width = 180,
            Height = 28,
            Margin = new Padding(0, 6, 12, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(244, 247, 252),
            Padding = new Padding(10, 0, 0, 0)
        };
    }

    private static DataGridView CreateGridView()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeight = 32,
            GridColor = Color.FromArgb(220, 225, 232),
            EnableHeadersVisualStyles = false
        };
    }
}
