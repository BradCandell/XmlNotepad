//#define WHIDBEY_MENUS

using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Diagnostics;
using System.Text;
using Microsoft.XmlDiffPatch;

using MyContextMenu = System.Windows.Forms.ContextMenu;
using TopLevelMenuItemBaseType = System.Windows.Forms.MenuItem;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.Xml;

namespace XmlNotepad {
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class FormMain : System.Windows.Forms.Form, ISite {
        
        UndoManager undoManager;
        Settings settings;
        string[] args;
        DataFormats.Format urlFormat;
        private System.Windows.Forms.StatusBar statusBar1;       
        private System.Windows.Forms.StatusBarPanel statusBarPanelMessage;
        private System.Windows.Forms.StatusBarPanel statusBarPanelBusy;
        RecentFilesMenu recentFiles;        
        
        XsltViewer dynamicHelpViewer;
        bool loading;
        FormSearch search;
        IIntellisenseProvider ip;
        OpenFileDialog od;
        WebProxyService proxyService;
        bool firstActivate = true;
        int batch;
        bool includesExpanded;
        bool helpAvailableHint = true;
        Updater updater;
        System.CodeDom.Compiler.TempFileCollection tempFiles = new System.CodeDom.Compiler.TempFileCollection();
        private ContextMenuStrip contextMenu1;
        private ToolStripSeparator ctxMenuItem20;
        private ToolStripSeparator ctxMenuItem23;
        private ToolStripMenuItem ctxcutToolStripMenuItem;
        private ToolStripMenuItem ctxMenuItemCopy;
        private ToolStripMenuItem ctxMenuItemPaste;
        private ToolStripMenuItem ctxMenuItemExpand;
        private ToolStripMenuItem ctxMenuItemCollapse;
        private HelpProvider helpProvider1;
        private ToolStripMenuItem ctxElementToolStripMenuItem;
        private ToolStripMenuItem ctxElementBeforeToolStripMenuItem;
        private ToolStripMenuItem ctxElementAfterToolStripMenuItem;
        private ToolStripMenuItem ctxElementChildToolStripMenuItem;
        private ToolStripMenuItem ctxAttributeToolStripMenuItem;
        private ToolStripMenuItem ctxAttributeAfterToolStripMenuItem;
        private ToolStripMenuItem ctxAttributeChildToolStripMenuItem;
        private ToolStripMenuItem ctxTextToolStripMenuItem;
        private ToolStripMenuItem ctxTextBeforeToolStripMenuItem;
        private ToolStripMenuItem ctxTextAfterToolStripMenuItem;
        private ToolStripMenuItem ctxTextChildToolStripMenuItem;
        private ToolStripMenuItem ctxCommentToolStripMenuItem;
        private ToolStripMenuItem ctxCommentBeforeToolStripMenuItem;
        private ToolStripMenuItem ctxCommentAfterToolStripMenuItem;
        private ToolStripMenuItem ctxCommentChildToolStripMenuItem;
        private ToolStripMenuItem ctxCdataToolStripMenuItem;
        private ToolStripMenuItem ctxCdataBeforeToolStripMenuItem;
        private ToolStripMenuItem ctxCdataAfterToolStripMenuItem;
        private ToolStripMenuItem ctxCdataChildToolStripMenuItem;
        private ToolStripMenuItem ctxPIToolStripMenuItem;
        private ToolStripMenuItem ctxPIBeforeToolStripMenuItem;
        private ToolStripMenuItem ctxPIAfterToolStripMenuItem;
        private ToolStripMenuItem ctxPIChildToolStripMenuItem;
        private System.ComponentModel.IContainer components;
        private XmlCache model;

        private string undoLabel;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem ctxGotoDefinitionToolStripMenuItem;
        // ChangeTo Context menu...
        private ToolStripMenuItem changeToContextMenuItem; 
        private ToolStripMenuItem changeToAttributeContextMenuItem;
        private ToolStripMenuItem changeToTextContextMenuItem;
        private ToolStripMenuItem changeToCDATAContextMenuItem;
        private ToolStripMenuItem changeToCommentContextMenuItem;
        private ToolStripMenuItem changeToProcessingInstructionContextMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem1;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem13;
        private ToolStripMenuItem insertToolStripMenuItem1;
        private ToolStripMenuItem duplicateToolStripMenuItem1;
        private ToolStripMenuItem ctxAttributeBeforeToolStripMenuItem;
        private XmlTreeView xmlTreeView1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem openToolStripMenuItem;
        private string redoLabel;


        public FormMain() {

            this.settings = new Settings();
            this.model = (XmlCache)GetService(typeof(XmlCache));
            this.ip = (XmlIntellisenseProvider)GetService(typeof(XmlIntellisenseProvider));
            //this.model = new XmlCache((ISynchronizeInvoke)this);
            this.undoManager = new UndoManager(1000);
            this.undoManager.StateChanged += new EventHandler(undoManager_StateChanged);

            this.SuspendLayout();

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Separated out so we can have virtual CreateTreeView without causing WinForms designer to barf.
            InitializeTreeView();

            
            this.xmlTreeView1.Dock = System.Windows.Forms.DockStyle.Fill;            

            this.ResumeLayout();

            InitializeHelp(this.helpProvider1);

            model.FileChanged += new EventHandler(OnFileChanged);
            model.ModelChanged += new EventHandler<ModelChangedEventArgs>(OnModelChanged);


            //this.resizer.Pane1 = this.xmlTreeView1;
            
            
            //this.Controls.SetChildIndex(this.resizer, 0);

            // populate default settings and provide type info.
            Font f = new Font("Courier New", 10, FontStyle.Regular);
            this.Font = f;
            this.settings["Font"] = f;
            System.Collections.Hashtable colors = new System.Collections.Hashtable();
            colors["Element"] = Color.FromArgb(0, 64, 128);
            colors["Attribute"] = Color.Maroon;
            colors["Text"] = Color.Black;
            colors["Comment"] = Color.Green;
            colors["PI"] = Color.Purple;
            colors["CDATA"] = Color.Gray;
            colors["Background"] = Color.White;
            colors["ContainerBackground"] = Color.AliceBlue;

            this.settings["Colors"] = colors;
            this.settings["FileName"] = new Uri("/",UriKind.RelativeOrAbsolute);
            this.settings["WindowBounds"] = new Rectangle(0,0,0,0);
            this.settings["TaskListSize"] = 0;
            this.settings["TreeViewSize"] = 0;
            this.settings["RecentFiles"] = new Uri[0];
            this.settings["SchemaCache"] = this.model.SchemaCache;
            this.settings["SearchWindowLocation"] = new Point(0, 0);
            this.settings["SearchSize"] = new Size(0, 0);
            this.settings["FindMode"] = false;
            this.settings["SearchXPath"] = false;
            this.settings["SearchWholeWord"] = false;
            this.settings["SearchRegex"] = false;
            this.settings["SearchMatchCase"] = false;

            this.settings["LastUpdateCheck"] = DateTime.Now;
            this.settings["UpdateFrequency"] = TimeSpan.FromDays(20);
            this.settings["UpdateLocation"] = "http://www.lovettsoftware.com/downloads/xmlnotepad/Updates.xml";
            this.settings["UpdateEnabled"] = true;

            this.settings["AutoFormatOnSave"] = true;
            this.settings["IndentLevel"] = 2;
            this.settings["IndentChar"] = IndentChar.Space;
            this.settings["NewLineChars"] = UserSettings.Escape("\r\n");
            this.settings["Language"] = "";
            this.settings["NoByteOrderMark"] = false;

            this.settings["AppRegistered"] = false;
            this.settings["MaximumLineLength"] = 10000;
            this.settings["AutoFormatLongLines"] = false;
            this.settings["IgnoreDTD"] = false;

            this.settings.Changed += new SettingsEventHandler(settings_Changed);

            // now that we have a font, override the tabControlViews font setting.
            this.xmlTreeView1.Font = this.Font;

            // Event wiring
            this.xmlTreeView1.SetSite(this);
            this.xmlTreeView1.SelectionChanged += new EventHandler(treeView1_SelectionChanged);
            this.xmlTreeView1.NodeChanged += new EventHandler<NodeChangeEventArgs>(treeView1_NodeChanged);
            this.xmlTreeView1.KeyDown += new KeyEventHandler(treeView1_KeyDown);
            
            this.DragOver += new DragEventHandler(Form1_DragOver);
            this.xmlTreeView1.TreeView.DragOver += new DragEventHandler(Form1_DragOver);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.xmlTreeView1.TreeView.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.AllowDrop = true;

            this.urlFormat = DataFormats.GetFormat("UniformResourceLocatorW");

            ctxcutToolStripMenuItem.Click += new EventHandler(this.cutToolStripMenuItem_Click);
            ctxMenuItemCopy.Click += new EventHandler(this.copyToolStripMenuItem_Click);
            ctxMenuItemPaste.Click += new EventHandler(this.pasteToolStripMenuItem_Click);
            ctxMenuItemExpand.Click += new EventHandler(this.expandToolStripMenuItem_Click);
            ctxMenuItemCollapse.Click += new EventHandler(this.collapseToolStripMenuItem_Click);


            // now set in virtual InitializeHelp()
            // 
            // helpProvider1
            // 
            //this.helpProvider1.HelpNamespace = Application.StartupPath + "\\Help.chm";
            //this.helpProvider1.Site = this;

            this.ContextMenuStrip = this.contextMenu1;            
            New();

        }

        public FormMain(string[] args)
            : this() {
            this.args = args;
        }


        public XmlCache Model {
            get { return model; }
            set { model = value; }
        }

        

        public XmlTreeView XmlTreeView {
            get { return xmlTreeView1; }
            set { xmlTreeView1 = value; }
        }

        void InitializeTreeView() {
            // Now remove the WinForm designer generated tree view and call the virtual CreateTreeView method
            // so a subclass of this form can plug in their own tree view.
            this.Controls.Remove(this.xmlTreeView1);

            this.xmlTreeView1.Close();

            this.xmlTreeView1 = CreateTreeView();

            // 
            // xmlTreeView1
            // 
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            resources.ApplyResources(this.xmlTreeView1, "xmlTreeView1");
            this.xmlTreeView1.ResizerPosition = 200;
            this.xmlTreeView1.BackColor = System.Drawing.Color.White;
            this.xmlTreeView1.Location = new System.Drawing.Point(0, 52);
            this.xmlTreeView1.Name = "xmlTreeView1";
            this.xmlTreeView1.SelectedNode = null;
            this.xmlTreeView1.TabIndex = 1;

            this.Controls.Add(this.xmlTreeView1);
            this.Controls.SetChildIndex(this.xmlTreeView1, 0);

        }

        protected virtual void InitializeHelp(HelpProvider hp) {
            hp.SetHelpNavigator(this, HelpNavigator.TableOfContents);
            hp.Site = this;
            // in case subclass has already set HelpNamespace
            if (string.IsNullOrEmpty(hp.HelpNamespace))
            {
                string path = Application.StartupPath + "\\Help\\Help.htm";
                hp.HelpNamespace = path;
            }
        }

        void FocusNextPanel(bool reverse) {
            Control[] panels = new Control[] { this.xmlTreeView1.TreeView, this.xmlTreeView1.NodeTextView };
            for (int i = 0; i < panels.Length; i++) {
                Control c = panels[i];
                if (c.ContainsFocus) {
                    int j = i + 1;
                    if (reverse) {
                        j = i - 1;
                        if (j < 0) j = panels.Length - 1;
                    } else if (j >= panels.Length) {
                        j = 0;
                    }
                    SelectTreeView();
                    panels[j].Focus();
                    break;
                }
            }            
        }

        void treeView1_KeyDown(object sender, KeyEventArgs e) {
            // Note if e.SuppressKeyPress is true, then this event is bubbling up from
            // the TextEditorOverlay - so we have to be careful not to interfere with
            // intellisense editing here unless we really want to.  Turns out the following
            // keystrokes all want to interrupt intellisense. 
            if (e.Handled) return;
            switch (e.KeyCode) {
                case Keys.Space:
                    if ((e.Modifiers & Keys.Control) == Keys.Control) {
                        this.xmlTreeView1.Commit();
                        Rectangle r = this.xmlTreeView1.TreeView.Bounds;
                        XmlTreeNode node = this.xmlTreeView1.SelectedNode;
                        if (node != null) {
                            r = node.LabelBounds;
                            r.Offset(this.xmlTreeView1.TreeView.ScrollPosition);
                        }
                        r = this.xmlTreeView1.RectangleToScreen(r);
                        this.contextMenu1.Show(r.Left + (r.Width / 2), r.Top + (r.Height / 2));
                    }
                    break;
                case Keys.F3:
                    if (this.search != null) {
                        this.xmlTreeView1.Commit();
                        this.search.FindAgain(e.Shift);
                        e.Handled = true;
                    }
                    break;
                case Keys.F6:
                    FocusNextPanel((e.Modifiers & Keys.Shift) != 0);
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnLoad(EventArgs e) {
            this.updater = new Updater(this.settings);
            this.updater.Title = this.Caption;
            this.updater.UpdateRequired += new EventHandler(OnUpdateRequired);
            LoadConfig();
            base.OnLoad(e);
        }

        void OnUpdateRequired(object sender, EventArgs e) {
            ISynchronizeInvoke si = (ISynchronizeInvoke)this;
            if (si.InvokeRequired) {
                // get on the right thread.
                si.Invoke(new EventHandler(OnUpdateRequired), new object[1] { e });
                return;
            }
        }
        
        void toolStripMenuItemUpdate_Click(object sender, EventArgs e) {         
            if (MessageBox.Show(string.Format(SR.UpdateAvailable, this.caption, updater.Version), 
                SR.UpdateAvailableCaption, MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation) == DialogResult.Yes) {
                Utilities.OpenUrl(this.Handle, this.updater.DownloadPage);
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            this.xmlTreeView1.Commit();
            if (this.model.Dirty){
                SelectTreeView();
                DialogResult rc = MessageBox.Show(this, SR.SaveChangesPrompt, SR.SaveChangesCaption, 
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (rc == DialogResult.Yes){
                    this.Save();
                } else if (rc == DialogResult.Cancel){
                    e.Cancel = true;
                    return;
                }
            }
            SaveConfig();
            base.OnClosing (e);
        }

        protected override void OnClosed(EventArgs e) {
            this.xmlTreeView1.Close();
            base.OnClosed(e);
            CleanupTempFiles();
            if (this.updater != null) {
                this.updater.Dispose();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent) {
            Size s = this.ClientSize;
            int w = s.Width;
            int h = s.Height;
            int top = 0;
            int sbHeight = 0;
            if (this.statusBar1.Visible) {
                sbHeight = this.statusBar1.Height;
                this.statusBar1.Size = new Size(w, sbHeight);
            }
            

            //this.tabControlViews.Size = new Size(w, h - top - sbHeight - this.tabControlLists.Height - this.resizer.Height);
            //this.tabControlViews.Padding = new Point(0, 0);
            //this.xmlTreeView1.Location = new Point(0, top);
            //this.xmlTreeView1.Size = new Size(w, h - top - sbHeight - this.tabControlViews.Height);
            //this.resizer.Size = new Size(w, this.resizer.Height);
            //this.resizer.Location = new Point(0, top + this.tabControlViews.Height);
            //this.taskList.Size = new Size(w, this.taskList.Height);
            //this.taskList.Location = new Point(0, top + this.xmlTreeView1.Height + this.resizer.Height);
            //this.tabControlLists.Size = new Size(w, this.tabControlLists.Height);
            //this.tabControlLists.Location = new Point(0, top + this.tabControlViews.Height + this.resizer.Height);
            base.OnLayout(levent);
        }

        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing ) {
            if( disposing ) {
                if (components != null) {
                    components.Dispose();
                }
                if (this.settings != null) {
                    this.settings.Dispose();
                    this.settings = null;
                }
                if (this.model != null) {
                    this.model.Dispose();
                    this.model = null;
                }
                IDisposable d = this.ip as IDisposable;
                if (d != null) {
                    d.Dispose();
                }
                this.ip = null;
            }
            base.Dispose( disposing );
        }

        protected virtual XmlTreeView CreateTreeView() {
            return new XmlTreeView();
        }


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [STAThread]
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripMenuItem changeToElementContextMenuItem;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statusBarPanelMessage = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelBusy = new System.Windows.Forms.StatusBarPanel();
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToAttributeContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToTextContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToCDATAContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToCommentContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToProcessingInstructionContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxGotoDefinitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuItem20 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxElementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxElementBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxElementAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxElementChildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxAttributeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxAttributeBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxAttributeAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxAttributeChildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTextBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTextAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxTextChildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCommentBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCommentAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCommentChildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCdataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCdataBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCdataAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCdataChildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxPIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxPIBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxPIAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxPIChildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuItem23 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxMenuItemExpand = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuItemCollapse = new System.Windows.Forms.ToolStripMenuItem();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.xmlTreeView1 = new XmlNotepad.XmlTreeView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeToElementContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelMessage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelBusy)).BeginInit();
            this.contextMenu1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // changeToElementContextMenuItem
            // 
            changeToElementContextMenuItem.Name = "changeToElementContextMenuItem";
            resources.ApplyResources(changeToElementContextMenuItem, "changeToElementContextMenuItem");
            changeToElementContextMenuItem.Click += new System.EventHandler(this.changeToElementContextMenuItem_Click);
            // 
            // statusBar1
            // 
            resources.ApplyResources(this.statusBar1, "statusBar1");
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanelMessage,
            this.statusBarPanelBusy});
            this.helpProvider1.SetShowHelp(this.statusBar1, ((bool)(resources.GetObject("statusBar1.ShowHelp"))));
            this.statusBar1.ShowPanels = true;
            // 
            // statusBarPanelMessage
            // 
            this.statusBarPanelMessage.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            resources.ApplyResources(this.statusBarPanelMessage, "statusBarPanelMessage");
            // 
            // statusBarPanelBusy
            // 
            resources.ApplyResources(this.statusBarPanelBusy, "statusBarPanelBusy");
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxcutToolStripMenuItem,
            this.ctxMenuItemCopy,
            this.ctxMenuItemPaste,
            this.toolStripMenuItem13,
            this.deleteToolStripMenuItem1,
            this.insertToolStripMenuItem1,
            this.renameToolStripMenuItem,
            this.duplicateToolStripMenuItem1,
            this.changeToContextMenuItem,
            this.toolStripSeparator3,
            this.ctxGotoDefinitionToolStripMenuItem,
            this.ctxMenuItem20,
            this.ctxElementToolStripMenuItem,
            this.ctxAttributeToolStripMenuItem,
            this.ctxTextToolStripMenuItem,
            this.ctxCommentToolStripMenuItem,
            this.ctxCdataToolStripMenuItem,
            this.ctxPIToolStripMenuItem,
            this.ctxMenuItem23,
            this.ctxMenuItemExpand,
            this.ctxMenuItemCollapse});
            this.contextMenu1.Name = "contextMenuStrip1";
            this.helpProvider1.SetShowHelp(this.contextMenu1, ((bool)(resources.GetObject("contextMenu1.ShowHelp"))));
            resources.ApplyResources(this.contextMenu1, "contextMenu1");
            // 
            // ctxcutToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxcutToolStripMenuItem, "ctxcutToolStripMenuItem");
            this.ctxcutToolStripMenuItem.Name = "ctxcutToolStripMenuItem";
            // 
            // ctxMenuItemCopy
            // 
            resources.ApplyResources(this.ctxMenuItemCopy, "ctxMenuItemCopy");
            this.ctxMenuItemCopy.Name = "ctxMenuItemCopy";
            // 
            // ctxMenuItemPaste
            // 
            resources.ApplyResources(this.ctxMenuItemPaste, "ctxMenuItemPaste");
            this.ctxMenuItemPaste.Name = "ctxMenuItemPaste";
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            resources.ApplyResources(this.toolStripMenuItem13, "toolStripMenuItem13");
            // 
            // deleteToolStripMenuItem1
            // 
            resources.ApplyResources(this.deleteToolStripMenuItem1, "deleteToolStripMenuItem1");
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // insertToolStripMenuItem1
            // 
            this.insertToolStripMenuItem1.Name = "insertToolStripMenuItem1";
            resources.ApplyResources(this.insertToolStripMenuItem1, "insertToolStripMenuItem1");
            this.insertToolStripMenuItem1.Click += new System.EventHandler(this.insertToolStripMenuItem1_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            resources.ApplyResources(this.renameToolStripMenuItem, "renameToolStripMenuItem");
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // duplicateToolStripMenuItem1
            // 
            this.duplicateToolStripMenuItem1.Name = "duplicateToolStripMenuItem1";
            resources.ApplyResources(this.duplicateToolStripMenuItem1, "duplicateToolStripMenuItem1");
            this.duplicateToolStripMenuItem1.Click += new System.EventHandler(this.duplicateToolStripMenuItem1_Click);
            // 
            // changeToContextMenuItem
            // 
            this.changeToContextMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            changeToElementContextMenuItem,
            this.changeToAttributeContextMenuItem,
            this.changeToTextContextMenuItem,
            this.changeToCDATAContextMenuItem,
            this.changeToCommentContextMenuItem,
            this.changeToProcessingInstructionContextMenuItem});
            this.changeToContextMenuItem.Name = "changeToContextMenuItem";
            resources.ApplyResources(this.changeToContextMenuItem, "changeToContextMenuItem");
            // 
            // changeToAttributeContextMenuItem
            // 
            this.changeToAttributeContextMenuItem.Name = "changeToAttributeContextMenuItem";
            resources.ApplyResources(this.changeToAttributeContextMenuItem, "changeToAttributeContextMenuItem");
            this.changeToAttributeContextMenuItem.Click += new System.EventHandler(this.changeToAttributeContextMenuItem_Click);
            // 
            // changeToTextContextMenuItem
            // 
            this.changeToTextContextMenuItem.Name = "changeToTextContextMenuItem";
            resources.ApplyResources(this.changeToTextContextMenuItem, "changeToTextContextMenuItem");
            this.changeToTextContextMenuItem.Click += new System.EventHandler(this.changeToTextToolStripMenuItem_Click);
            // 
            // changeToCDATAContextMenuItem
            // 
            this.changeToCDATAContextMenuItem.Name = "changeToCDATAContextMenuItem";
            resources.ApplyResources(this.changeToCDATAContextMenuItem, "changeToCDATAContextMenuItem");
            this.changeToCDATAContextMenuItem.Click += new System.EventHandler(this.changeToCDATAContextMenuItem_Click);
            // 
            // changeToCommentContextMenuItem
            // 
            this.changeToCommentContextMenuItem.Name = "changeToCommentContextMenuItem";
            resources.ApplyResources(this.changeToCommentContextMenuItem, "changeToCommentContextMenuItem");
            this.changeToCommentContextMenuItem.Click += new System.EventHandler(this.changeToCommentContextMenuItem_Click);
            // 
            // changeToProcessingInstructionContextMenuItem
            // 
            this.changeToProcessingInstructionContextMenuItem.Name = "changeToProcessingInstructionContextMenuItem";
            resources.ApplyResources(this.changeToProcessingInstructionContextMenuItem, "changeToProcessingInstructionContextMenuItem");
            this.changeToProcessingInstructionContextMenuItem.Click += new System.EventHandler(this.changeToProcessingInstructionContextMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // ctxGotoDefinitionToolStripMenuItem
            // 
            this.ctxGotoDefinitionToolStripMenuItem.Name = "ctxGotoDefinitionToolStripMenuItem";
            resources.ApplyResources(this.ctxGotoDefinitionToolStripMenuItem, "ctxGotoDefinitionToolStripMenuItem");
            this.ctxGotoDefinitionToolStripMenuItem.Click += new System.EventHandler(this.ctxGotoDefinitionToolStripMenuItem_Click);
            // 
            // ctxMenuItem20
            // 
            this.ctxMenuItem20.Name = "ctxMenuItem20";
            resources.ApplyResources(this.ctxMenuItem20, "ctxMenuItem20");
            // 
            // ctxElementToolStripMenuItem
            // 
            this.ctxElementToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxElementBeforeToolStripMenuItem,
            this.ctxElementAfterToolStripMenuItem,
            this.ctxElementChildToolStripMenuItem});
            this.ctxElementToolStripMenuItem.Name = "ctxElementToolStripMenuItem";
            resources.ApplyResources(this.ctxElementToolStripMenuItem, "ctxElementToolStripMenuItem");
            // 
            // ctxElementBeforeToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxElementBeforeToolStripMenuItem, "ctxElementBeforeToolStripMenuItem");
            this.ctxElementBeforeToolStripMenuItem.Name = "ctxElementBeforeToolStripMenuItem";
            this.ctxElementBeforeToolStripMenuItem.Click += new System.EventHandler(this.elementBeforeToolStripMenuItem_Click);
            // 
            // ctxElementAfterToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxElementAfterToolStripMenuItem, "ctxElementAfterToolStripMenuItem");
            this.ctxElementAfterToolStripMenuItem.Name = "ctxElementAfterToolStripMenuItem";
            this.ctxElementAfterToolStripMenuItem.Click += new System.EventHandler(this.elementAfterToolStripMenuItem_Click);
            // 
            // ctxElementChildToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxElementChildToolStripMenuItem, "ctxElementChildToolStripMenuItem");
            this.ctxElementChildToolStripMenuItem.Name = "ctxElementChildToolStripMenuItem";
            this.ctxElementChildToolStripMenuItem.Click += new System.EventHandler(this.elementChildToolStripMenuItem_Click);
            // 
            // ctxAttributeToolStripMenuItem
            // 
            this.ctxAttributeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxAttributeBeforeToolStripMenuItem,
            this.ctxAttributeAfterToolStripMenuItem,
            this.ctxAttributeChildToolStripMenuItem});
            this.ctxAttributeToolStripMenuItem.Name = "ctxAttributeToolStripMenuItem";
            resources.ApplyResources(this.ctxAttributeToolStripMenuItem, "ctxAttributeToolStripMenuItem");
            // 
            // ctxAttributeBeforeToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxAttributeBeforeToolStripMenuItem, "ctxAttributeBeforeToolStripMenuItem");
            this.ctxAttributeBeforeToolStripMenuItem.Name = "ctxAttributeBeforeToolStripMenuItem";
            this.ctxAttributeBeforeToolStripMenuItem.Click += new System.EventHandler(this.attributeBeforeToolStripMenuItem_Click);
            // 
            // ctxAttributeAfterToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxAttributeAfterToolStripMenuItem, "ctxAttributeAfterToolStripMenuItem");
            this.ctxAttributeAfterToolStripMenuItem.Name = "ctxAttributeAfterToolStripMenuItem";
            this.ctxAttributeAfterToolStripMenuItem.Click += new System.EventHandler(this.attributeAfterToolStripMenuItem_Click);
            // 
            // ctxAttributeChildToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxAttributeChildToolStripMenuItem, "ctxAttributeChildToolStripMenuItem");
            this.ctxAttributeChildToolStripMenuItem.Name = "ctxAttributeChildToolStripMenuItem";
            this.ctxAttributeChildToolStripMenuItem.Click += new System.EventHandler(this.attributeChildToolStripMenuItem_Click);
            // 
            // ctxTextToolStripMenuItem
            // 
            this.ctxTextToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxTextBeforeToolStripMenuItem,
            this.ctxTextAfterToolStripMenuItem,
            this.ctxTextChildToolStripMenuItem});
            this.ctxTextToolStripMenuItem.Name = "ctxTextToolStripMenuItem";
            resources.ApplyResources(this.ctxTextToolStripMenuItem, "ctxTextToolStripMenuItem");
            // 
            // ctxTextBeforeToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxTextBeforeToolStripMenuItem, "ctxTextBeforeToolStripMenuItem");
            this.ctxTextBeforeToolStripMenuItem.Name = "ctxTextBeforeToolStripMenuItem";
            this.ctxTextBeforeToolStripMenuItem.Click += new System.EventHandler(this.textBeforeToolStripMenuItem_Click);
            // 
            // ctxTextAfterToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxTextAfterToolStripMenuItem, "ctxTextAfterToolStripMenuItem");
            this.ctxTextAfterToolStripMenuItem.Name = "ctxTextAfterToolStripMenuItem";
            this.ctxTextAfterToolStripMenuItem.Click += new System.EventHandler(this.textAfterToolStripMenuItem_Click);
            // 
            // ctxTextChildToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxTextChildToolStripMenuItem, "ctxTextChildToolStripMenuItem");
            this.ctxTextChildToolStripMenuItem.Name = "ctxTextChildToolStripMenuItem";
            this.ctxTextChildToolStripMenuItem.Click += new System.EventHandler(this.textChildToolStripMenuItem_Click);
            // 
            // ctxCommentToolStripMenuItem
            // 
            this.ctxCommentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxCommentBeforeToolStripMenuItem,
            this.ctxCommentAfterToolStripMenuItem,
            this.ctxCommentChildToolStripMenuItem});
            this.ctxCommentToolStripMenuItem.Name = "ctxCommentToolStripMenuItem";
            resources.ApplyResources(this.ctxCommentToolStripMenuItem, "ctxCommentToolStripMenuItem");
            // 
            // ctxCommentBeforeToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxCommentBeforeToolStripMenuItem, "ctxCommentBeforeToolStripMenuItem");
            this.ctxCommentBeforeToolStripMenuItem.Name = "ctxCommentBeforeToolStripMenuItem";
            this.ctxCommentBeforeToolStripMenuItem.Click += new System.EventHandler(this.commentBeforeToolStripMenuItem_Click);
            // 
            // ctxCommentAfterToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxCommentAfterToolStripMenuItem, "ctxCommentAfterToolStripMenuItem");
            this.ctxCommentAfterToolStripMenuItem.Name = "ctxCommentAfterToolStripMenuItem";
            this.ctxCommentAfterToolStripMenuItem.Click += new System.EventHandler(this.commentAfterToolStripMenuItem_Click);
            // 
            // ctxCommentChildToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxCommentChildToolStripMenuItem, "ctxCommentChildToolStripMenuItem");
            this.ctxCommentChildToolStripMenuItem.Name = "ctxCommentChildToolStripMenuItem";
            this.ctxCommentChildToolStripMenuItem.Click += new System.EventHandler(this.commentChildToolStripMenuItem_Click);
            // 
            // ctxCdataToolStripMenuItem
            // 
            this.ctxCdataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxCdataBeforeToolStripMenuItem,
            this.ctxCdataAfterToolStripMenuItem,
            this.ctxCdataChildToolStripMenuItem});
            this.ctxCdataToolStripMenuItem.Name = "ctxCdataToolStripMenuItem";
            resources.ApplyResources(this.ctxCdataToolStripMenuItem, "ctxCdataToolStripMenuItem");
            // 
            // ctxCdataBeforeToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxCdataBeforeToolStripMenuItem, "ctxCdataBeforeToolStripMenuItem");
            this.ctxCdataBeforeToolStripMenuItem.Name = "ctxCdataBeforeToolStripMenuItem";
            this.ctxCdataBeforeToolStripMenuItem.Click += new System.EventHandler(this.cdataBeforeToolStripMenuItem_Click);
            // 
            // ctxCdataAfterToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxCdataAfterToolStripMenuItem, "ctxCdataAfterToolStripMenuItem");
            this.ctxCdataAfterToolStripMenuItem.Name = "ctxCdataAfterToolStripMenuItem";
            this.ctxCdataAfterToolStripMenuItem.Click += new System.EventHandler(this.cdataAfterToolStripMenuItem_Click);
            // 
            // ctxCdataChildToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxCdataChildToolStripMenuItem, "ctxCdataChildToolStripMenuItem");
            this.ctxCdataChildToolStripMenuItem.Name = "ctxCdataChildToolStripMenuItem";
            this.ctxCdataChildToolStripMenuItem.Click += new System.EventHandler(this.cdataChildToolStripMenuItem_Click);
            // 
            // ctxPIToolStripMenuItem
            // 
            this.ctxPIToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxPIBeforeToolStripMenuItem,
            this.ctxPIAfterToolStripMenuItem,
            this.ctxPIChildToolStripMenuItem});
            this.ctxPIToolStripMenuItem.Name = "ctxPIToolStripMenuItem";
            resources.ApplyResources(this.ctxPIToolStripMenuItem, "ctxPIToolStripMenuItem");
            // 
            // ctxPIBeforeToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxPIBeforeToolStripMenuItem, "ctxPIBeforeToolStripMenuItem");
            this.ctxPIBeforeToolStripMenuItem.Name = "ctxPIBeforeToolStripMenuItem";
            this.ctxPIBeforeToolStripMenuItem.Click += new System.EventHandler(this.PIBeforeToolStripMenuItem_Click);
            // 
            // ctxPIAfterToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxPIAfterToolStripMenuItem, "ctxPIAfterToolStripMenuItem");
            this.ctxPIAfterToolStripMenuItem.Name = "ctxPIAfterToolStripMenuItem";
            this.ctxPIAfterToolStripMenuItem.Click += new System.EventHandler(this.PIAfterToolStripMenuItem_Click);
            // 
            // ctxPIChildToolStripMenuItem
            // 
            resources.ApplyResources(this.ctxPIChildToolStripMenuItem, "ctxPIChildToolStripMenuItem");
            this.ctxPIChildToolStripMenuItem.Name = "ctxPIChildToolStripMenuItem";
            this.ctxPIChildToolStripMenuItem.Click += new System.EventHandler(this.PIChildToolStripMenuItem_Click);
            // 
            // ctxMenuItem23
            // 
            this.ctxMenuItem23.Name = "ctxMenuItem23";
            resources.ApplyResources(this.ctxMenuItem23, "ctxMenuItem23");
            // 
            // ctxMenuItemExpand
            // 
            this.ctxMenuItemExpand.Name = "ctxMenuItemExpand";
            resources.ApplyResources(this.ctxMenuItemExpand, "ctxMenuItemExpand");
            // 
            // ctxMenuItemCollapse
            // 
            this.ctxMenuItemCollapse.Name = "ctxMenuItemCollapse";
            resources.ApplyResources(this.ctxMenuItemCollapse, "ctxMenuItemCollapse");
            // 
            // xmlTreeView1
            // 
            resources.ApplyResources(this.xmlTreeView1, "xmlTreeView1");
            this.xmlTreeView1.BackColor = System.Drawing.SystemColors.Window;
            this.xmlTreeView1.Name = "xmlTreeView1";
            this.xmlTreeView1.ResizerPosition = 200;
            this.xmlTreeView1.ScrollPosition = new System.Drawing.Point(0, 0);
            this.xmlTreeView1.SelectedNode = null;
            this.helpProvider1.SetShowHelp(this.xmlTreeView1, ((bool)(resources.GetObject("xmlTreeView1.ShowHelp"))));
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click_1);
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.xmlTreeView1);
            this.Controls.Add(this.statusBar1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelMessage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelBusy)).EndInit();
            this.contextMenu1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void checkUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.updater.CheckNow();
        }

        protected virtual void TabControlViews_Selected(object sender, NoBorderTabControlEventArgs e) {
            
        }

        #endregion

        void EnableFileMenu(){
            bool hasFile = (model.FileName != null);
        }

        void SelectTreeView() {
            if (!xmlTreeView1.ContainsFocus) {
                xmlTreeView1.Focus();
            }
        }

        public virtual void New(){
            SelectTreeView();
            if (!SaveIfDirty(true))
                return;  
            model.Clear();
            includesExpanded = false;
            EnableFileMenu();
            this.settings["FileName"] = new Uri("/", UriKind.RelativeOrAbsolute);
        }

        protected virtual IIntellisenseProvider CreateIntellisenseProvider(XmlCache model, ISite site) {
            return new XmlIntellisenseProvider(this.model, site);
        }

        protected override object GetService(Type service) {
            if (service == typeof(UndoManager)){
                return this.undoManager;
            } else if (service == typeof(SchemaCache)) {
                return this.model.SchemaCache;
            } else if (service == typeof(TreeView)) {
                XmlTreeView view = (XmlTreeView)GetService(typeof(XmlTreeView));
                return view.TreeView;
            } else if (service == typeof(XmlTreeView)) {
                if (this.xmlTreeView1 == null) {
                    this.xmlTreeView1 = this.CreateTreeView();
                }
                return this.xmlTreeView1;
            } else if (service == typeof(XmlCache)) {
                if (null == this.model)
                {
                    this.model = new XmlCache((IServiceProvider)this, (ISynchronizeInvoke)this);
                }
                return this.model;
            } else if (service == typeof(Settings)){
                return this.settings;
            } else if (service == typeof(IIntellisenseProvider)) {
                if (this.ip == null) this.ip = CreateIntellisenseProvider(this.model, this);
                return this.ip;
            } else if (service == typeof(HelpProvider)) {
                return this.helpProvider1;
            } else if (service == typeof(WebProxyService)) {
                if (this.proxyService == null)
                    this.proxyService = new WebProxyService((IServiceProvider)this);
                return this.proxyService;
            } else if (service == typeof(UserSettings)) {
                return new UserSettings(this.settings);
            }
            return base.GetService (service);
        }

        public OpenFileDialog OpenFileDialog {
            get { return this.od; }
        }

        public virtual void Open() {
            SelectTreeView();
            if (!SaveIfDirty(true))
                return;
            if (od == null) od = new OpenFileDialog();
            if (model.FileName != null) {
                Uri uri = new Uri(model.FileName);
                if (uri.Scheme == "file"){
                    od.FileName = model.FileName;
                }
            }
            string filter = SR.OpenFileFilter;
            od.Filter = filter;
            string[] parts = filter.Split('|');
            int index = -1;
            for (int i = 1, n = parts.Length; i < n; i += 2 ) {
                if (parts[i] == "*.*") {
                    index = (i / 2)+1;
                    break;
                }
            }
            od.FilterIndex = index;
            if (od.ShowDialog(this) == DialogResult.OK){
                Open(od.FileName);
            }
        }

        public virtual void ShowStatus(string msg) {
            this.statusBarPanelMessage.Text = msg;
        }

        public virtual void Open(string filename) {
            try {
                // Make sure you've called SaveIfDirty before calling this method.
                string ext = System.IO.Path.GetExtension(filename).ToLowerInvariant();
                if (ext == ".csv")
                {
                    ImportCsv(filename);
                }
                else
                {
                    InternalOpen(filename);
                }
            } catch (Exception e){
                if (MessageBox.Show(this,
                    string.Format(SR.LoadErrorPrompt, filename, e.Message),
                    SR.LoadErrorCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) {
                    OpenNotepad(filename);
                }
            }
        }

        private void ImportCsv(string filename)
        {
            FormCsvImport importForm = new XmlNotepad.FormCsvImport();
            importForm.FileName = filename;
            if (importForm.ShowDialog() == DialogResult.OK)
            {
                // then import it for real...
                using (StreamReader reader = new StreamReader(filename))
                {
                    string xmlFile = Path.Combine(Path.GetDirectoryName(filename),
                        Path.GetFileNameWithoutExtension(filename) + ".xml");

                    XmlCsvReader csv = new XmlCsvReader(reader, new Uri(filename), new NameTable());
                    csv.Delimiter = importForm.Deliminter;
                    csv.FirstRowHasColumnNames = importForm.FirstRowIsHeader;

                    includesExpanded = false;
                    DateTime start = DateTime.Now;
                    this.model.Load(csv, xmlFile);
                    DateTime finish = DateTime.Now;
                    TimeSpan diff = finish - start;
                    string s = diff.ToString();
                    this.settings["FileName"] = this.model.Location;
                    this.UpdateCaption();
                    ShowStatus(string.Format(SR.LoadedTimeStatus, s));
                    EnableFileMenu();
                    //this.recentFiles.AddRecentFile(this.model.Location);
                    SelectTreeView();
                }   
            }
        }

        private void InternalOpen(string filename) {
            includesExpanded = false;
            DateTime start = DateTime.Now;            
            this.model.Load(filename);
            DateTime finish = DateTime.Now;
            TimeSpan diff = finish - start;
            string s = diff.ToString();
            this.settings["FileName"] = this.model.Location;
            this.UpdateCaption();
            ShowStatus(string.Format(SR.LoadedTimeStatus, s));
            EnableFileMenu();
            //this.recentFiles.AddRecentFile(this.model.Location);
            SelectTreeView();
        }

        bool CheckXIncludes() {
            if (includesExpanded) {
                if (MessageBox.Show(this, SR.SaveExpandedIncludesPrompt, SR.SaveExpandedIncludesCaption,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) {
                    return false;
                }
                includesExpanded = false;
            }
            return true;
        }

        public virtual bool SaveIfDirty(bool prompt) {
            if (model.Dirty){
                if (prompt){
                    SelectTreeView();
                    DialogResult rc = MessageBox.Show(this, SR.SaveChangesPrompt,
                        SR.SaveChangesCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);                
                    if (rc == DialogResult.Cancel){
                        return false;
                    } else if (rc == DialogResult.Yes){
                        return Save();
                    }
                } else {
                    return Save();
                }
            }
            return true;
        }

        public virtual bool Save() {
            this.xmlTreeView1.Commit();
            if (!CheckXIncludes()) return false;                
            string fname = model.FileName;
            if (fname == null){
                SaveAs();
            } else {
                try
                {
                    this.xmlTreeView1.BeginSave();
                    if (CheckReadOnly(fname)) {
                        model.Save();
                        ShowStatus(SR.SavedStatus);
                    }
                } catch (Exception e){
                    MessageBox.Show(this, e.Message, SR.SaveErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.xmlTreeView1.EndSave();
                }
            }
            return true;
        }

        public bool CheckReadOnly(string fname) {
            if (model.IsReadOnly(fname)) {
                SelectTreeView();
                if (MessageBox.Show(this, string.Format(SR.ReadOnly, fname),
                    SR.ReadOnlyCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes) {
                    model.MakeReadWrite(fname);
                    return true;
                } else {
                    return false;
                }
            }
            return true;    
        }

        public virtual void Save(string newName) {
            this.xmlTreeView1.Commit();
            this.xmlTreeView1.BeginSave();
            try {
                bool hasFile = (model.FileName != null);
                if (!hasFile && string.IsNullOrEmpty(newName)) {
                    SaveAs();
                }
                if (CheckReadOnly(newName)) {
                    model.Save(newName);
                    UpdateCaption();
                    ShowStatus(SR.SavedStatus);
                    this.settings["FileName"] = model.Location;
                    EnableFileMenu();
                }
            } catch (Exception e){
                MessageBox.Show(this, e.Message, SR.SaveErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }
            finally
            {
                this.xmlTreeView1.EndSave();
            }
        }

        public virtual void SaveAs() {
            SelectTreeView();
            SaveFileDialog sd = new SaveFileDialog();
            if (model.IsFile) sd.FileName = model.FileName;
            sd.Filter = SR.SaveAsFilter;
            if (sd.ShowDialog(this) == DialogResult.OK){
                string fname = sd.FileName;
                if (CheckReadOnly(fname)) {
                    Save(fname);
                }
            }
        }

        string caption = null;

        public string Caption {
            get {
                if (string.IsNullOrEmpty(caption))
                    caption = SR.MainFormTitle;
                return caption; }
            set { caption = value; }
        }

        public virtual void UpdateCaption() {
            string caption = this.Caption + " - " + model.FileName;
            if (this.model.Dirty){
                caption += "*";
            }            
            this.Text = caption;
            ShowStatus("");
        }

        void OnFileChanged(object sender, EventArgs e) {
            if (!prompting) OnFileChanged();
        }

        bool prompting = false;

        protected virtual void OnFileChanged() {
            prompting = true;
            try {
                if (this.WindowState == FormWindowState.Minimized) {
                    this.WindowState = FormWindowState.Normal;
                }
                SelectTreeView();
                if (MessageBox.Show(this, SR.FileChagedOnDiskPrompt, SR.FileChagedOnDiskCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) {
                    string location = this.model.Location.LocalPath;
                    this.model.Clear();
                    this.Open(location);                                                     
                }
            } finally {
                prompting = false;
            }
        }

        private void undoManager_StateChanged(object sender, EventArgs e) {
            this.ShowStatus("");
            Command cmd = this.undoManager.Peek();
            cmd = this.undoManager.Current;
        }

        public virtual string ConfigFile {
            get { 
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                Debug.Assert(!string.IsNullOrEmpty(path));
                return path + @"\Microsoft\Xml Notepad\XmlNotepad.settings";
            }
        }

        public virtual void LoadConfig() {
            string path = null;
            this.loading = true;
            if (this.args != null && this.args.Length > 0) {
                // When user passes arguments we skip the config file
                // This is for unit testing where we need consistent config!
                path = this.args[0];
                this.settings.FileName = this.ConfigFile;
            } else {
                if (File.Exists(this.ConfigFile)) {
                    settings.Load(this.ConfigFile);


                    string newLines = (string)this.settings["NewLineChars"];

                    Uri location = (Uri)this.settings["FileName"];
                    // Load up the last file we were editing before - if it is local and still exists.
                    if (location != null && location.OriginalString != "/" && location.IsFile && File.Exists(location.LocalPath)) {
                        path = location.LocalPath;
                    }

                    string updates = (string)this.settings["UpdateLocation"];
                    if (updates == "http://download.microsoft.com/download/6/e/e/6eef2361-33d4-48a2-b52e-5827c7f2ad68/Updates.xml" ||
                        string.IsNullOrEmpty(updates))
                    {
                        this.settings["UpdateLocation"] = "http://www.lovettsoftware.com/downloads/xmlnotepad/Updates.xml";
                    }
                    
                }
            }            
            this.loading = false;
        }

        public virtual void SaveConfig() {
            this.settings.StopWatchingFileChanges();
            Rectangle r = (this.WindowState == FormWindowState.Normal) ? this.Bounds : this.RestoreBounds;
            this.settings["WindowBounds"] = r;
            this.settings["Font"] = this.Font;
            this.settings["TreeViewSize"] = this.xmlTreeView1.ResizerPosition;
            this.settings.Save(this.ConfigFile);
        }

        #region  ISite implementation
        IComponent ISite.Component{
            get { return this; }
        }

        public static Type ResourceType { get { return typeof(SR); } }

        string ISite.Name {
            get { return this.Name; }
            set { this.Name = value; } 
        }

        IContainer ISite.Container {
            get { return this.Container; }
        }

        bool ISite.DesignMode {
            get { return this.DesignMode;}
        }
        object IServiceProvider.GetService(Type serviceType) {
            return this.GetService(serviceType);
        }
        #endregion

        void OnModelChanged(object sender, ModelChangedEventArgs e) {
            if (e.ModelChangeType == ModelChangeType.Reloaded) {
                this.undoManager.Clear();
            }
            if (e.ModelChangeType == ModelChangeType.BeginBatchUpdate) {
                batch++;
            } else if (e.ModelChangeType == ModelChangeType.EndBatchUpdate) {
                batch--;
            }
            if (batch == 0) OnModelChanged();
        }

        protected virtual void OnModelChanged() {
            UpdateCaption();
        }

        private void settings_Changed(object sender, string name) {
            // Make sure it's on the right thread...
            ISynchronizeInvoke si = (ISynchronizeInvoke)this;
            if (si.InvokeRequired) {
                si.BeginInvoke(new SettingsEventHandler(OnSettingsChanged),
                    new object[] { sender, name });
            } else {
                OnSettingsChanged(sender, name);
            }
        }

        protected virtual void OnSettingsChanged(object sender, string name) {        
            switch (name){
                case "File":
                    this.settings.Reload(); // just do it!!                    
                    break;
                case "WindowBounds":
                    if (loading) { // only if loading first time!
                        Rectangle r = (Rectangle)this.settings["WindowBounds"];
                        if (!r.IsEmpty) {
                            Screen s = Screen.FromRectangle(r);
                            if (s.Bounds.Contains(r)) {
                                this.Bounds = r;
                                this.StartPosition = FormStartPosition.Manual;
                            }
                        }
                    }
                    break;
                case "TreeViewSize":
                    int pos = (int)this.settings["TreeViewSize"];
                    if (pos != 0) {
                        this.xmlTreeView1.ResizerPosition = pos;
                    }
                    break;
                case "Font":
                    this.Font = (Font)this.settings["Font"];
                    break;
                case "RecentFiles":
                    Uri[] files = (Uri[])this.settings["RecentFiles"];
                    if (files != null) {
                        this.recentFiles.SetFiles(files);
                    }
                    break;
            }
        }

        public void SaveErrors(string filename) {
            
        }

        void OnRecentFileSelected(object sender, RecentFileEventArgs e) {
            if (!this.SaveIfDirty(true))
                return;                                       
            string fileName = e.FileName;
            Open(fileName);
        }

        private void treeView1_SelectionChanged(object sender, EventArgs e) {
        }

        private void treeView1_NodeChanged(object sender, NodeChangeEventArgs e) {
        }

        

        void EnableNodeItems(XmlNodeType nt, ToolStripMenuItem c1, ToolStripMenuItem m1, ToolStripMenuItem c2, ToolStripMenuItem m2, ToolStripMenuItem c3, ToolStripMenuItem m3){
            c1.Enabled = m1.Enabled = this.xmlTreeView1.CanInsertNode(InsertPosition.Before, nt);
            c2.Enabled = m2.Enabled = this.xmlTreeView1.CanInsertNode(InsertPosition.After, nt);
            c3.Enabled = m3.Enabled = this.xmlTreeView1.CanInsertNode(InsertPosition.Child, nt);
        }

        protected virtual void OpenNotepad(string path) {
            if (this.SaveIfDirty(true)){
                string sysdir = Environment.SystemDirectory;
                string notepad = Path.Combine(sysdir, "notepad.exe");
                if (File.Exists(notepad)){
                    ProcessStartInfo pi = new ProcessStartInfo(notepad, path);
                    Process.Start(pi);
                }
            }
        }


		protected override void OnActivated(EventArgs e) {
            if (firstActivate) {
                firstActivate = false;
            }
            if (this.xmlTreeView1.TreeView.IsEditing) {
                this.xmlTreeView1.TreeView.Focus();
            } else if (this.xmlTreeView1.NodeTextView.IsEditing) {
                this.xmlTreeView1.NodeTextView.Focus();
            }
		}

        void taskList_Navigate(object sender, Task task) {
            XmlNode node = task.Data as XmlNode;
            if (node != null) {
                XmlTreeNode tn = this.xmlTreeView1.FindNode(node);
                if (tn != null) {
                    this.xmlTreeView1.SelectedNode = tn;
                    this.SelectTreeView();
                }
            }
        }

        private void Form1_DragOver(object sender, DragEventArgs e) {
            IDataObject data = e.Data;
            if (data.GetDataPresent(DataFormats.FileDrop) || 
                data.GetDataPresent(this.urlFormat.Name) ||
                data.GetDataPresent("UniformResourceLocator"))
            {
                if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
            }
            return;
        }


        private void Form1_DragDrop(object sender, DragEventArgs e) {
            IDataObject data = e.Data;
            if (data.GetDataPresent(DataFormats.FileDrop)){
                Array a = data.GetData(DataFormats.FileDrop) as Array;
                if (a != null){
                    if (a.Length>0 && a.GetValue(0) is string){
                        string filename = (string)a.GetValue(0);
                        if (!this.SaveIfDirty(true))
                            return;
                        this.Open(filename);
                    }
                }
            } else if (data.GetDataPresent(this.urlFormat.Name)){
                Stream stm = data.GetData(this.urlFormat.Name) as Stream;
                if (stm != null) {
                    try {
                        // Note: for some reason sr.ReadToEnd doesn't work right.
                        StreamReader sr = new StreamReader(stm, Encoding.Unicode);
                        StringBuilder sb = new StringBuilder();
                        while (true) {
                            int i = sr.Read();
                            if (i != 0) {
                                sb.Append(Convert.ToChar(i));
                            } else {
                                break;
                            }
                        }
                        string url = sb.ToString();
                        if (!this.SaveIfDirty(true))
                            return;
                        this.Open(url);
                    } catch (Exception){
                    }
                }
            }
            else if (data.GetDataPresent("UniformResourceLocator"))
            {
                string uri = (string)data.GetData(DataFormats.UnicodeText);
                if (!string.IsNullOrEmpty(uri))
                {
                    this.Open(uri);
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.CancelEdit();
            New();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.CancelEdit();
            Open();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView(); 
            if (model.Dirty) {                
                if (MessageBox.Show(this, SR.DiscardChanges, SR.DiscardChangesCaption,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel) {
                    return;
                }                    
            }
            Open(this.model.FileName);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.Commit();
            Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.Commit();
            SaveAs();
        }

        private void menuItemRecentFiles_Click(object sender, EventArgs e) {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.Commit();
            this.Close();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                this.xmlTreeView1.CancelEdit();
                this.undoManager.Undo();
                SelectTreeView();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.Message, SR.UndoError, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                if (this.xmlTreeView1.Commit())
                    this.undoManager.Redo();
                SelectTreeView();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.Message, SR.RedoError, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.xmlTreeView1.Commit()) 
                this.xmlTreeView1.Cut();
            SelectTreeView();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.Commit();
            this.xmlTreeView1.Copy();
            SelectTreeView();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.Paste(InsertPosition.Child);
            SelectTreeView();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            DeleteSelectedNode();
        }

        void DeleteSelectedNode() {
            this.xmlTreeView1.Commit();
            this.xmlTreeView1.Delete();
            SelectTreeView();
        }

        private void repeatToolStripMenuItem_Click(object sender, EventArgs e) {
            this.RepeatSelectedNode();
        }

        void RepeatSelectedNode() {
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.Insert();
            SelectTreeView();
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e) {
            DuplicateSelectedNode();
        }

        void DuplicateSelectedNode() {
            try {
                if (this.xmlTreeView1.Commit())
                    this.xmlTreeView1.Duplicate();
                SelectTreeView();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.Message, SR.DuplicateErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        private void upToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.xmlTreeView1.Commit())                    
                this.xmlTreeView1.NudgeNode(this.xmlTreeView1.SelectedNode, NudgeDirection.Up);
            SelectTreeView();
        }

        private void downToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.NudgeNode(this.xmlTreeView1.SelectedNode, NudgeDirection.Down);
            SelectTreeView();
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.NudgeNode(this.xmlTreeView1.SelectedNode, NudgeDirection.Left);
            SelectTreeView();
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.NudgeNode(this.xmlTreeView1.SelectedNode, NudgeDirection.Right);
            SelectTreeView();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e) {            
            Search(false);
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e) {
            Search(true);
        }

        void Search(bool replace) {

            if (search == null || !search.Visible) {
                search = new FormSearch(search, (ISite)this);
                search.Owner = this;
            } else {
                search.Activate();
            }
            search.Target = new XmlTreeViewFindTarget(this.xmlTreeView1);
            search.ReplaceMode = replace;

            if (!search.Visible) {
                search.Show(this); // modeless
            }
        }

        private void expandToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            XmlTreeNode s = this.xmlTreeView1.SelectedNode;
            if (s != null) {
                s.ExpandAll();
            }
        }

        private void collapseToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            XmlTreeNode s = this.xmlTreeView1.SelectedNode;
            if (s != null) {
                s.CollapseAll();
            }
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.xmlTreeView1.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.xmlTreeView1.CollapseAll();
        }

        
        private void sourceToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenNotepad(this.model.FileName);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
            string oldLocation = (string)settings["UpdateLocation"];
            FormOptions options = new FormOptions();
            options.Owner = this;
            options.Site = this;
            if (options.ShowDialog(this) == DialogResult.OK) {
                this.updater.OnUserChange(oldLocation);
            }
        }


        private void contentsToolStripMenuItem_Click(object sender, EventArgs e) {
            Help.ShowHelp(this, this.helpProvider1.HelpNamespace, HelpNavigator.TableOfContents);
        }

        private void indexToolStripMenuItem_Click(object sender, EventArgs e) {
            Help.ShowHelp(this, this.helpProvider1.HelpNamespace, HelpNavigator.Index);
        }

        private void aboutXMLNotepadToolStripMenuItem_Click(object sender, EventArgs e) {
            FormAbout frm = new FormAbout();
            frm.ShowDialog(this);
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e) {
            this.xmlTreeView1.CancelEdit();
            this.New();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e) {
            this.xmlTreeView1.CancelEdit();
            this.Open();
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e) {
            this.xmlTreeView1.Commit();
            this.Save();
        }

        private void toolStripButtonUndo_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.undoManager.Undo();
        }

        private void toolStripButtonRedo_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.undoManager.Redo();
        }

        private void toolStripButtonCut_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.xmlTreeView1.Cut();
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.xmlTreeView1.Copy();
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.xmlTreeView1.Paste(InsertPosition.Child);
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.xmlTreeView1.CancelEdit();
            this.xmlTreeView1.Delete();
        }

        private void toolStripButtonNudgeUp_Click(object sender, EventArgs e) {
            this.upToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonNudgeDown_Click(object sender, EventArgs e) {
            this.downToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonNudgeLeft_Click(object sender, EventArgs e) {
            this.leftToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonNudgeRight_Click(object sender, EventArgs e) {
            this.rightToolStripMenuItem_Click(sender, e);
        }

        // Insert Menu Items.

        private void elementAfterToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.After, XmlNodeType.Element);
        }

        private void elementBeforeToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Before, XmlNodeType.Element);
        }

        private void elementChildToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Child, XmlNodeType.Element);
        }

        private void attributeBeforeToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Before, XmlNodeType.Attribute);
        }

        private void attributeAfterToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.After, XmlNodeType.Attribute);
        }

        private void attributeChildToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Child, XmlNodeType.Attribute);
        }

        private void textBeforeToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Before, XmlNodeType.Text);
        }

        private void textAfterToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.After, XmlNodeType.Text);
        }

        private void textChildToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Child, XmlNodeType.Text);
        }

        private void commentBeforeToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Before, XmlNodeType.Comment);
        }

        private void commentAfterToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.After, XmlNodeType.Comment);
        }

        private void commentChildToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Child, XmlNodeType.Comment);
        }

        private void cdataBeforeToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Before, XmlNodeType.CDATA);
        }

        private void cdataAfterToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.After, XmlNodeType.CDATA);
        }

        private void cdataChildToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Child, XmlNodeType.CDATA);
        }

        private void PIBeforeToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Before, XmlNodeType.ProcessingInstruction);
        }

        private void PIAfterToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.After, XmlNodeType.ProcessingInstruction);
        }

        private void PIChildToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            if (this.xmlTreeView1.Commit())
                this.xmlTreeView1.InsertNode(InsertPosition.Child, XmlNodeType.ProcessingInstruction);
        }

        void Launch(string exeFileName, string args) {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = exeFileName;
            info.Arguments = "/offset " + args;
            Process p = new Process();
            p.StartInfo = info;
            if (!p.Start()) {
                MessageBox.Show(this, string.Format(SR.ErrorCreatingProcessPrompt, exeFileName), SR.LaunchErrorPrompt, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e){
            this.SaveIfDirty(true);
            this.OpenNewWindow(this.model.FileName);
        }


        private void schemasToolStripMenuItem_Click(object sender, EventArgs e) {
            FormSchemas frm = new FormSchemas();
            frm.Owner = this;
            frm.Site = this;
            if (frm.ShowDialog(this) == DialogResult.OK) {
                OnModelChanged();
            }
        }

        private void compareXMLFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = SR.SaveAsFilter;
            if (ofd.ShowDialog(this) == DialogResult.OK) {
                string secondFile = ofd.FileName;
                try {
                    DoCompare(secondFile);
                } catch (Exception ex) {
                    MessageBox.Show(this, ex.Message, SR.XmlDiffErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        string GetEmbeddedString(string name)
        {
            using (Stream stream = typeof(XmlNotepad.FormMain).Assembly.GetManifestResourceStream(name))
            {
                StreamReader sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// The html header used by XmlNotepad.
        /// </summary>
        /// <param name="sourceXmlFile">name of baseline xml data</param>
        /// <param name="changedXmlFile">name of file being compared</param>
        /// <param name="resultHtml">Output file</param>
        private void SideBySideXmlNotepadHeader(
            string sourceXmlFile,
            string changedXmlFile,
            TextWriter resultHtml) {

            // this initializes the html
            resultHtml.WriteLine("<html><head>");
            resultHtml.WriteLine("<style TYPE='text/css'>");
            resultHtml.WriteLine(GetEmbeddedString("XmlNotepad.Resources.XmlReportStyles.css"));
            resultHtml.WriteLine("</style>");
            resultHtml.WriteLine("</head>");
            resultHtml.WriteLine(GetEmbeddedString("XmlNotepad.Resources.XmlDiffHeader.html"));

            resultHtml.WriteLine(string.Format(SR.XmlDiffBody,
                    System.IO.Path.GetDirectoryName(sourceXmlFile),
                    System.IO.Path.GetFileName(sourceXmlFile),
                    System.IO.Path.GetDirectoryName(changedXmlFile),
                    System.IO.Path.GetFileName(changedXmlFile)
            ));

        }

        void CleanupTempFiles() {
            try {
                this.tempFiles.Delete();
            } catch {
            }
        }

        private void DoCompare(string changed) {
            CleanupTempFiles();

            // todo: add UI for setting XmlDiffOptions.
            this.xmlTreeView1.Commit();
            this.SaveIfDirty(false);
            string filename = this.model.FileName;

            XmlDocument original = this.model.Document;
            XmlDocument doc = new XmlDocument();

            XmlReaderSettings settings = model.GetReaderSettings();
            using (XmlReader reader = XmlReader.Create(changed, settings)) {
                doc.Load(reader);
            }

            string startupPath = Application.StartupPath;
            //output diff file.
            string diffFile = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".xml");
            this.tempFiles.AddFile(diffFile, false);

            bool isEqual = false;
            XmlTextWriter diffWriter = new XmlTextWriter(diffFile, Encoding.UTF8);
            diffWriter.Formatting = Formatting.Indented;
            using (diffWriter) {
                XmlDiff diff = new XmlDiff();
                isEqual = diff.Compare(original, doc, diffWriter);
                diff.Options = XmlDiffOptions.None;
            }

            if (isEqual) {
                //This means the files were identical for given options.
                MessageBox.Show(this, SR.FilesAreIdenticalPrompt, SR.FilesAreIdenticalCaption,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return; 
            }

            string tempFile = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".htm");
            tempFiles.AddFile(tempFile, false);
            
            using (XmlReader diffGram = XmlReader.Create(diffFile, settings)) {
                XmlDiffView diffView = new XmlDiffView();
                diffView.Load(new XmlTextReader(filename), diffGram);
                using (TextWriter htmlWriter = new StreamWriter(tempFile, false, Encoding.UTF8)) {
                    SideBySideXmlNotepadHeader(this.model.FileName, changed, htmlWriter);
                    diffView.GetHtml(htmlWriter);
                    htmlWriter.WriteLine("</body></html>");
                }
            }

            Utilities.OpenUrl(this.Handle, tempFile);
        }
        
        string ApplicationPath {
            get {
                string path = Application.ExecutablePath;
                if (path.EndsWith("vstesthost.exe", StringComparison.CurrentCultureIgnoreCase)) {
                    // must be running UnitTests
                    Uri baseUri = new Uri(this.GetType().Assembly.Location);
                    Uri resolved = new Uri(baseUri, @"..\..\..\Application\bin\debug\XmlNotepad.exe");
                    path = resolved.LocalPath;
                }
                return path;
            }
        }

        public virtual void OpenNewWindow(string path){
            if (!string.IsNullOrEmpty(path)) {
                Uri uri = new Uri(path);
                if (uri.IsFile) {
                    path = uri.LocalPath;
                    if (!File.Exists(path)) {
                        DialogResult dr = MessageBox.Show(
                            String.Format(SR.CreateFile, path), SR.CreateNodeFileCaption,
                            MessageBoxButtons.OKCancel);
                        if (dr.Equals(DialogResult.OK)) {
                            try {
                                XmlDocument include = new XmlDocument();
                                include.InnerXml = "<root/>";
                                include.Save(path);
                            } catch (Exception e) {
                                MessageBox.Show(this, e.Message, SR.SaveErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        } else {
                            return;
                        }
                    }
                }
            }
            Launch(this.ApplicationPath, "\"" + path + "\"");
        }

        private void GotoDefinition() {
            SelectTreeView();
            this.SaveIfDirty(true);

            XmlTreeNode node = xmlTreeView1.SelectedNode;
            if (node == null) return;

            string ipath = node.GetDefinition();

            if (!string.IsNullOrEmpty(ipath)) {
                OpenNewWindow(ipath);
            }
            
        }

        private void gotoDefinitionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.GotoDefinition();
        }

        private void ctxGotoDefinitionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.GotoDefinition();
        }

        private void expandXIncludesToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectTreeView();
            this.model.ExpandIncludes();
            includesExpanded = true;
        }

        private void exportErrorsToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveAsErrors();
        }

        void SaveAsErrors() {
            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = SR.SaveAsFilter;
            sd.Title = SR.SaveErrorsCaption;
            if (sd.ShowDialog(this) == DialogResult.OK) {
                string fname = sd.FileName;
                if (CheckReadOnly(fname)) {
                    SaveErrors(fname);
                }
            }
        }

        private void changeToAttributeToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Attribute);
        }

        private void changeToTextToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Text);
        }

        private void changeToCDATAToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.CDATA);
        }

        private void changeToCommentToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Comment);
        }

        private void changeToProcessingInstructionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.ProcessingInstruction);
        }

        private void changeToElementContextMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Element);
        }

        private void changeToAttributeContextMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Attribute);
        }

        private void changeToTextToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Text);
        }

        private void changeToCDATAContextMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.CDATA);
        }

        private void changeToCommentContextMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Comment);
        }

        private void changeToProcessingInstructionContextMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.ChangeTo(XmlNodeType.ProcessingInstruction);
        }

        private void incrementalSearchToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.StartIncrementalSearch();
        }

        private void renameToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.xmlTreeView1.BeginEditNodeName();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e) {
            this.xmlTreeView1.BeginEditNodeName();
        }

        private void insertToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.RepeatSelectedNode();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.DeleteSelectedNode();
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e) {
            this.DuplicateSelectedNode();
        }

        private void fileAssociationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool registered = (bool)this.settings["AppRegistered"];
            if (!registered)
            {
                this.settings["AppRegistered"] = true;

                byte[] file = SR.XmlNotepadRegistration;

                string path = Path.Combine(Path.GetTempPath(), "XmlNotepadRegistration.exe");
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(file, 0, file.Length);
                }

                Process p = Process.Start(path, string.Format("\"{0}\" \"{1}\" \"{2}\"", Application.ExecutablePath, SR.AppProgId, SR.AppDescription));
                p.WaitForExit();
            }

            var assocUI = new ApplicationAssociationRegistrationUI();
            try
            {
                assocUI.LaunchAdvancedAssociationUI(SR.AppProgId);
            }
            catch
            {
                var message = string.Format("Could not display the file association manager. Please repair {0} and try again.", this.Text);
                MessageBox.Show(this, message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Marshal.ReleaseComObject(assocUI);
            }

        }

        private void elementToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.xmlTreeView1.ChangeTo(XmlNodeType.Element);
        }

        private void statsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.xmlTreeView1.Commit();
            this.SaveIfDirty(false);
            XmlStats xs = new XmlStats();

            string tempFile = Path.Combine(Path.GetTempPath(),
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".htm");
            tempFiles.AddFile(tempFile, false);

            using (TextWriter resultHtml = new StreamWriter(tempFile, false, Encoding.UTF8))
            {
                resultHtml.WriteLine("<html><head>");
                resultHtml.WriteLine("<style TYPE='text/css'>");
                resultHtml.WriteLine(GetEmbeddedString("XmlNotepad.Resources.XmlReportStyles.css"));
                resultHtml.WriteLine("</style>");
                resultHtml.WriteLine("</head>");
                resultHtml.WriteLine(@"        
    <div id='header'>        
        <h2>XML Statistics: " + this.model.FileName + @"</h2>
    </div>
    <div id='main'><div class='code'><xmp>");

                xs.ProcessFiles(new string[1] { this.model.FileName }, true, resultHtml, "\n");

                resultHtml.WriteLine("</xmp></div></body></html>");
            }


            Utilities.OpenUrl(this.Handle, tempFile);
        }

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e) {
            Open();           
        }
    }

}