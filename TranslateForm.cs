/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2022 Ingo Herbote
 * https://www.yetanotherforum.net/
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * https://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.TranslateApp;

using YAF.TranslateApp.Properties;

using BorderStyle = System.Windows.Forms.BorderStyle;

/// <summary>
/// The translate form.
/// </summary>
public partial class TranslateForm : Form
{
    /// <summary>
    /// The cell local resource.
    /// </summary>
    private readonly Cell cellLocalResource;

    /// <summary>
    /// The cell local resource red.
    /// </summary>
    private readonly Cell cellLocalResourceRed;

    /// <summary>
    /// The translations.
    /// </summary>
    private List<Translation> translations = new();

    /// <summary>
    /// The row count
    /// </summary>
    private int rowCount;

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateForm"/> class.
    /// </summary>
    public TranslateForm()
    {
        var border = new DevAge.Drawing.BorderLine(Color.Black, 1);
        var cellBorder = new DevAge.Drawing.RectangleBorder(border, border);

        this.cellLocalResourceRed = new Cell
                                        {
                                            Font = this.ResourceHeaderFont,
                                            TextAlignment = DevAge.Drawing.ContentAlignment.TopCenter,
                                            ForeColor = Color.Red,
                                            WordWrap = true,
                                            Border = cellBorder
                                        };
        this.cellLocalResource = new Cell
                                     {
                                         Font = this.ResourceHeaderFont,
                                         TextAlignment = DevAge.Drawing.ContentAlignment.TopCenter,
                                         WordWrap = true,
                                         Border = cellBorder
                                     };
        this.InitializeComponent();
    }

    #endregion;

    /// <summary>
    /// Gets the Header separator row font style
    /// </summary>
    private Font PageHeaderFont { get; } = new(SystemFonts.DefaultFont, FontStyle.Bold);

    /// <summary>
    /// Gets the Column 1 (Resource tag) font style
    /// </summary>
    private Font ResourceHeaderFont { get; } = new(SystemFonts.DefaultFont, FontStyle.Bold);

    /// <summary>Gets or sets a value indicating whether [destination translation file changed].</summary>
    private bool DestinationTranslationFileChanged { get; set; }

    /// <summary>
    /// Gets the List of namespaces for Resources in destination translation file
    /// </summary>
    private StringDictionary ResourcesNamespaces { get; } = new();

    /// <summary>
    /// Gets the List of attributes for Resources in destination translation file
    /// </summary>
    private StringDictionary ResourcesAttributes { get; } = new();

    /// <summary>
    /// Populate translation tables
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void PopulateTranslationsClick(object sender, EventArgs e)
    {
        this.PopulateTranslations(this.tbxSourceTranslationFile.Text, this.tbxDestinationTranslationFile.Text);
    }

    /// <summary>
    /// Load source translation
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void LoadSourceTranslationClick(object sender, EventArgs e)
    {
        var fileName = GetTranslationFileName("Select a File as Source Translation", "english.xml");

        if (!string.IsNullOrEmpty(fileName))
        {
            this.tbxSourceTranslationFile.Text = fileName;
        }

        if (!string.IsNullOrEmpty(this.tbxSourceTranslationFile.Text)
            && !string.IsNullOrEmpty(this.tbxDestinationTranslationFile.Text))
        {
            this.btnPopulateTranslations.Enabled = true;
        }

        var fileName2 = GetTranslationFileName("Select the Language File you want to Translate", null);

        if (!string.IsNullOrEmpty(fileName2))
        {
            this.tbxDestinationTranslationFile.Text = fileName2;
        }

        if (string.IsNullOrEmpty(this.tbxSourceTranslationFile.Text)
            || string.IsNullOrEmpty(this.tbxDestinationTranslationFile.Text))
        {
            return;
        }

        this.btnPopulateTranslations.Enabled = true;

        this.PopulateTranslations(this.tbxSourceTranslationFile.Text, this.tbxDestinationTranslationFile.Text);
    }

    /// <summary>
    /// Load destination, target, translation
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void LoadDestinationTranslationClick(object sender, EventArgs e)
    {
        var fileName = GetTranslationFileName("Select the Language File you want to Translate", null);

        if (!string.IsNullOrEmpty(fileName))
        {
            this.tbxDestinationTranslationFile.Text = fileName;
        }

        if (string.IsNullOrEmpty(this.tbxSourceTranslationFile.Text)
            || string.IsNullOrEmpty(this.tbxDestinationTranslationFile.Text))
        {
            return;
        }

        this.btnPopulateTranslations.Enabled = true;

        this.PopulateTranslations(this.tbxSourceTranslationFile.Text, this.tbxDestinationTranslationFile.Text);
    }

    /// <summary>
    /// Exit application
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void QuitClick(object sender, EventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// Save translation
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void SaveClick(object sender, EventArgs e)
    {
        this.DestinationTranslationFileChanged = false;
        this.SaveTranslation();
    }

    /// <summary>
    /// Check if translation changed and ask to save if changed
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void TranslateFormFormClosing(object sender, FormClosingEventArgs e)
    {
        if (!this.DestinationTranslationFileChanged)
        {
            return;
        }

        switch (MessageBox.Show(Resources.Save_Changes, Resources.Save, MessageBoxButtons.YesNoCancel))
        {
            case DialogResult.Cancel:
                e.Cancel = true;
                break;
            case DialogResult.No:
                break;
            case DialogResult.Yes:
                if (!this.SaveTranslation())
                {
                    e.Cancel = true;
                }

                break;
            case DialogResult.None:
            case DialogResult.OK:
            case DialogResult.Abort:
            case DialogResult.Retry:
            case DialogResult.Ignore:
            default:
                break;
        }
    }

    /// <summary>
    /// Set flag that translation has changed
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void TextChangedEvent(object sender, EventArgs e)
    {
        this.btnSave.Enabled = true;
    }

    /// <summary>
    /// Auto Translate The Selected Resource via Google Translator
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void MenuItemClick(object sender, EventArgs e)
    {
        var menuItem = (MenuItem)sender;

        var contextMenu = (ContextMenu)menuItem.Parent;

        var tbx = (TextBox)contextMenu.SourceControl;
        var tbt = (TextBoxTranslation)tbx.Tag;

        var region = this.grid1.Selection.GetSelectionRegion();
        var poss = region.GetCellsPositions();

        foreach (var t in from t in poss
                          let cell = this.grid1.GetCell(t) as SourceGrid.Cells.Cell
                          where cell != null
                          select t)
        {
            GetCell(this.grid1, t).View = tbt.SrcResourceValue.Equals(tbx.Text, StringComparison.OrdinalIgnoreCase)
                                              ? this.cellLocalResourceRed
                                              : this.cellLocalResource;
        }

        // Update Translations List
        this.translations
            .Find(check => check.PageName.Equals(tbt.PageName) && check.ResourceName.Equals(tbt.ResourceName))
            .LocalizedValue = tbx.Text;

        // tlpTranslations.Focus();
    }

    /// <summary>
    /// Compare source and destination values on focus lost and indicate (guess) whether text is translated or not
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void TbxLostFocus(object sender, EventArgs e)
    {
        var tbx = (TextBox)sender;
        var tbt = (TextBoxTranslation)tbx.Tag;

        tbx.ForeColor = tbt.SrcResourceValue.Equals(tbx.Text, StringComparison.OrdinalIgnoreCase)
                            ? Color.Red
                            : Color.Black;

        var region = this.grid1.Selection.GetSelectionRegion();
        var poss = region.GetCellsPositions();

        foreach (var t in from t in poss
                          let cell = this.grid1.GetCell(t) as SourceGrid.Cells.Cell
                          where cell != null
                          select t)
        {
            GetCell(this.grid1, t).View = tbt.SrcResourceValue.Equals(tbx.Text, StringComparison.OrdinalIgnoreCase)
                                              ? this.cellLocalResourceRed
                                              : this.cellLocalResource;
        }

        // Update Translations List
        this.translations
            .Find(check => check.PageName.Equals(tbt.PageName) && check.ResourceName.Equals(tbt.ResourceName))
            .LocalizedValue = tbx.Text;

        // tlpTranslations.Focus();
    }

    /// <summary>
    /// cast cell on position pos to Cell
    /// </summary>
    /// <param name="grid">The grid.</param>
    /// <param name="pos">The position.</param>
    /// <returns>Returns Cell.</returns>
    private static SourceGrid.Cells.Cell GetCell(GridVirtual grid, Position pos)
    {
        return grid.GetCell(pos) as SourceGrid.Cells.Cell;
    }

    /// <summary>
    /// Remove all Resources with the same Name and Page
    /// </summary>
    /// <typeparam name="T">The Typed Parameter</typeparam>
    /// <param name="list">The list.</param>
    /// <returns>Returns the Cleaned List.</returns>
    private static List<T> RemoveDuplicateSections<T>(IEnumerable<T> list)
        where T : Translation
    {
        var finalList = new List<T>();

        foreach (var item1 in list.Where(
                     item1 => finalList.Find(
                                  check => check.PageName.Equals(item1.PageName)
                                           && check.ResourceName.Equals(item1.ResourceName)) == null))
        {
            finalList.Add(item1);
        }

        return finalList;
    }

    /// <summary>
    /// Show open file dialog and return single filename
    /// </summary>
    /// <param name="title">
    /// The title.
    /// </param>
    /// <param name="fileName">
    /// The File Name.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    private static string GetTranslationFileName(string title, string fileName)
    {
        string result = null;

        var ofd = new OpenFileDialog
                      {
                          CheckFileExists = true,
                          CheckPathExists = true,
                          Multiselect = false,
                          Filter = Resources.XmlFilter,
                          Title = title,
                          FileName = fileName
                      };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            result = ofd.FileName;
        }

        return result;
    }

    /// <summary>
    /// Wraps creation of translation controls.
    /// </summary>
    /// <param name="srcFile">
    /// The source File.
    /// </param>
    /// <param name="dstFile">
    /// The destination File.
    /// </param>
    private void PopulateTranslations(string srcFile, string dstFile)
    {
        this.Cursor = Cursors.WaitCursor;

        this.rowCount = 0;

        this.grid1.Rows.Clear();
        this.grid1.Columns.Clear();

        this.grid1.BorderStyle = BorderStyle.FixedSingle;

        this.grid1.ColumnsCount = 3;

        this.grid1.Columns[0].AutoSizeMode = SourceGrid.AutoSizeMode.MinimumSize | SourceGrid.AutoSizeMode.Default;
        this.grid1.Columns[1].AutoSizeMode = SourceGrid.AutoSizeMode.MinimumSize | SourceGrid.AutoSizeMode.Default;
        this.grid1.Columns[2].AutoSizeMode = SourceGrid.AutoSizeMode.MinimumSize | SourceGrid.AutoSizeMode.Default;

        this.grid1.MinimumWidth = 100;

        this.grid1.AutoStretchColumnsToFitWidth = true;
        this.grid1.AutoSizeCells();
        this.grid1.Columns.StretchToFit();
        this.grid1.Columns.AutoSizeView();

        Settings.Default.SourceTranslation = srcFile;
        Settings.Default.DestinationTranslation = dstFile;

        Settings.Default.Save();

        this.tbxSourceTranslationFile.Text = srcFile;
        this.tbxDestinationTranslationFile.Text = dstFile;

        this.translations.Clear();

        this.CreateTranslateControls(Settings.Default.SourceTranslation, Settings.Default.DestinationTranslation);

        this.Cursor = Cursors.Default;

        this.btnSave.Enabled = true;
        this.btnAutoTranslate.Enabled = true;
    }

    /// <summary>
    /// Creates and populates the translation controls given source and destination file names.
    /// </summary>
    /// <param name="srcFile">
    /// The source File.
    /// </param>
    /// <param name="dstFile">
    /// The destination File.
    /// </param>
    private void CreateTranslateControls(string srcFile, string dstFile)
    {
        try
        {
            var docSrc = new XmlDocument();
            var docDst = new XmlDocument();

            docSrc.Load(srcFile);
            docDst.Load(dstFile);

            var navSrc = docSrc.DocumentElement.CreateNavigator();
            var navDst = docDst.DocumentElement.CreateNavigator();

            this.ResourcesNamespaces.Clear();
            if (navDst.MoveToFirstNamespace())
            {
                do
                {
                    this.ResourcesNamespaces.Add(navDst.Name, navDst.Value);
                }
                while (navDst.MoveToNextNamespace());
            }

            navDst.MoveToRoot();
            navDst.MoveToFirstChild();

            this.ResourcesAttributes.Clear();

            if (navSrc.MoveToFirstAttribute())
            {
                do
                {
                    if (!navSrc.Name.Equals("code"))
                    {
                        continue;
                    }
                }
                while (navSrc.MoveToNextAttribute());
            }

            navSrc.MoveToRoot();
            navSrc.MoveToFirstChild();

            if (navDst.MoveToFirstAttribute())
            {
                do
                {
                    this.ResourcesAttributes.Add(navDst.Name, navDst.Value);
                }
                while (navDst.MoveToNextAttribute());
            }

            var totalResourceCount = 0;
            var resourcesNotTranslated = 0;

            // int pageNodeCount = 0;
            // int resourceMissingCount = 0;
            navDst.MoveToRoot();
            navDst.MoveToFirstChild();

            foreach (XPathNavigator pageItemNavigator in navSrc.Select("page"))
            {
                // pageNodeCount++;
                // int pageResourceCount = 0;
                var pageNameAttributeValue = pageItemNavigator.GetAttribute("name", string.Empty);

                this.CreatePageResourceHeader(pageNameAttributeValue);

                var resourceItemCollection = pageItemNavigator.Select("Resource");

                this.progressBar.Maximum = resourceItemCollection.Count;
                this.progressBar.Minimum = 0;
                this.progressBar.Value = 0;

                foreach (XPathNavigator resourceItem in resourceItemCollection)
                {
                    this.progressBar.Value++;
                    totalResourceCount++;

                    var resourceTagAttributeValue = resourceItem.GetAttribute("tag", string.Empty);

                    var iteratorSe = navDst.Select(
                        $"/Resources/page[@name=\"{pageNameAttributeValue}\"]/Resource[@tag=\"{resourceTagAttributeValue}\"]");

                    if (iteratorSe.Count <= 0)
                    {
                        // pageResourceCount++;
                        // resourceMissingCount++;
                        this.DestinationTranslationFileChanged = true;

                        this.CreatePageResourceControl(
                            pageNameAttributeValue,
                            resourceTagAttributeValue,
                            resourceItem.Value,
                            resourceItem.Value);
                    }

                    while (iteratorSe.MoveNext())
                    {
                        // pageResourceCount++;
                        if (!iteratorSe.Current.Value.Equals(
                                resourceItem.Value,
                                StringComparison.OrdinalIgnoreCase))
                        {
                        }
                        else
                        {
                            resourcesNotTranslated++;
                        }

                        this.CreatePageResourceControl(
                            pageNameAttributeValue,
                            resourceTagAttributeValue,
                            resourceItem.Value,
                            iteratorSe.Current.Value);
                    }
                }

                // pageNodeCount++;
            }

            this.grid1.Columns.SetWidth(1, 100);
            this.grid1.Columns.StretchToFit();

            // Show Info
            this.toolStripStatusLabel1.Text =
                string.Format(Resources.TotalResources, totalResourceCount, resourcesNotTranslated);
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(Resources.ErrorLoading, ex.Message), Resources.Error, MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Creates a header row in the TableLayoutPanel. Header text is page section name in XML file.
    /// </summary>
    /// <param name="pageName">
    /// The page Name.
    /// </param>
    private void CreatePageResourceHeader(string pageName)
    {
        var pageHeader = new Cell
                             {
                                 BackColor = Color.LightBlue,
                                 Font = this.PageHeaderFont,
                                 TextAlignment = DevAge.Drawing.ContentAlignment.MiddleLeft,
                             };

        this.grid1.Rows.Insert(this.rowCount);

        this.grid1[this.rowCount, 0] = new SourceGrid.Cells.Cell(pageName) { View = pageHeader, ColumnSpan = 3 };
        this.grid1[this.rowCount, 1].AddController(new SourceGrid.Cells.Controllers.Unselectable());
        this.grid1.Rows[this.rowCount].Height = 50;

        this.rowCount++;

        this.grid1.Rows.Insert(this.rowCount);
        this.grid1[this.rowCount, 0] = new SourceGrid.Cells.ColumnHeader("Original Resource");
        this.grid1[this.rowCount, 1] = new SourceGrid.Cells.ColumnHeader("Resource Name");
        this.grid1[this.rowCount, 2] = new SourceGrid.Cells.ColumnHeader("Localized Resource");

        this.rowCount++;
    }

    /// <summary>
    /// Creates controls for column 1 (Resource tag) and column 2 (Resource value).
    /// </summary>
    /// <param name="pageName">
    /// The page Name.
    /// </param>
    /// <param name="resourceName">
    /// The resource Name.
    /// </param>
    /// <param name="srcResourceValue">
    /// The source Resource Value.
    /// </param>
    /// <param name="dstResourceValue">
    /// The destination Resource Value.
    /// </param>
    private void CreatePageResourceControl(
        string pageName,
        string resourceName,
        string srcResourceValue,
        string dstResourceValue)
    {
        var tbx = new SourceGrid.Cells.Editors.TextBox(typeof(string));

        tbx.Control.Text = dstResourceValue;
        tbx.Control.Multiline = true;

        if (tbx.Control.Text.Length > 30)
        {
            var height = 60 * (tbx.Control.Text.Length / 60);
            tbx.Control.Height = height;
        }

        var translation = new Translation
                              {
                                  PageName = pageName,
                                  ResourceName = resourceName,
                                  LocalizedValue = dstResourceValue
                              };

        this.translations.Add(translation);

        if (srcResourceValue.Equals(dstResourceValue, StringComparison.OrdinalIgnoreCase))
        {
            tbx.Control.ForeColor = Color.Red;
        }
        else
        {
            // Show only not translated
            if (this.checkPendingOnly.Checked)
            {
                return;
            }
        }

        tbx.Control.LostFocus += this.TbxLostFocus;
        tbx.Control.TextChanged += this.TextChangedEvent;

        var menuItem = new MenuItem { Text = Resources.AutoTranslate };

        menuItem.Click += this.MenuItemClick;

        var contextMenu = new ContextMenu();

        contextMenu.MenuItems.Add(menuItem);

        tbx.Control.ContextMenu = contextMenu;

        var border = new DevAge.Drawing.BorderLine(Color.Black, 1);
        var cellBorder = new DevAge.Drawing.RectangleBorder(border, border);

        tbx.Control.Tag = new TextBoxTranslation
                              {
                                  PageName = pageName,
                                  ResourceName = resourceName,
                                  SrcResourceValue = srcResourceValue
                              };

        var cellResourceValue = new Cell
                                    {
                                        Font = this.ResourceHeaderFont,
                                        TextAlignment = DevAge.Drawing.ContentAlignment.TopLeft,
                                        WordWrap = true,
                                        Border = cellBorder,
                                        BackColor = Color.LightGray
                                    };

        var cellResourceName = new Cell
                                   {
                                       Font = this.ResourceHeaderFont,
                                       TextAlignment = DevAge.Drawing.ContentAlignment.TopCenter,
                                       WordWrap = true,
                                       Border = cellBorder,
                                       BackColor = Color.LightGray
                                   };

        this.grid1.Rows.Insert(this.rowCount);
        this.grid1[this.rowCount, 0] =
            new SourceGrid.Cells.Cell(srcResourceValue, typeof(string)) { View = cellResourceValue };
        this.grid1[this.rowCount, 0].AddController(new SourceGrid.Cells.Controllers.Unselectable());

        this.grid1[this.rowCount, 1] =
            new SourceGrid.Cells.Cell(resourceName, typeof(string)) { View = cellResourceName };
        this.grid1[this.rowCount, 1].AddController(new SourceGrid.Cells.Controllers.Unselectable());

        if (tbx.Control.ForeColor.Equals(Color.Red))
        {
            this.grid1[this.rowCount, 2] =
                new SourceGrid.Cells.Cell(tbx.Control.Text) { View = this.cellLocalResourceRed, Editor = tbx };
        }
        else
        {
            this.grid1[this.rowCount, 2] =
                new SourceGrid.Cells.Cell(tbx.Control.Text) { View = this.cellLocalResource, Editor = tbx };
        }

        if (tbx.Control.Text.Length > 30)
        {
            var height = 60 * (tbx.Control.Text.Length / 60);

            this.grid1.Rows[this.rowCount].Height = height;
        }

        this.rowCount++;
    }

    /// <summary>
    /// Save translations back to original file.
    /// </summary>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    private bool SaveTranslation()
    {
        var result = true;

        var oldCount = this.translations.Count;

        this.translations = RemoveDuplicateSections(this.translations);

        var duplicates = oldCount - this.translations.Count;

        if (duplicates >= 1)
        {
            // MessageBox.Show(string.Format("{0} - Duplicate Entries Removed.", iDuplicates));
        }

        this.Cursor = Cursors.WaitCursor;

        try
        {
            var settings = new XmlWriterSettings
                               {
                                   Encoding = Encoding.UTF8,
                                   OmitXmlDeclaration = false,
                                   Indent = true,
                                   IndentChars = "\t"
                               };

            var xw = XmlWriter.Create(this.tbxDestinationTranslationFile.Text, settings);
            xw.WriteStartDocument();

            // <Resources>
            xw.WriteStartElement("Resources");

            foreach (string key in this.ResourcesNamespaces.Keys)
            {
                xw.WriteAttributeString("xmlns", key, null, this.ResourcesNamespaces[key]);
            }

            foreach (string key in this.ResourcesAttributes.Keys)
            {
                xw.WriteAttributeString(key, this.ResourcesAttributes[key]);
            }

            var currentPageName = string.Empty;

            foreach (var trans in this.translations)
            {
                // <page></page>
                if (!trans.PageName.Equals(currentPageName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(currentPageName))
                    {
                        xw.WriteFullEndElement();
                    }

                    currentPageName = trans.PageName;

                    xw.WriteStartElement("page");
                    xw.WriteAttributeString("name", currentPageName);
                }

                xw.WriteStartElement("Resource");
                xw.WriteAttributeString("tag", trans.ResourceName);
                xw.WriteString(trans.LocalizedValue);
                xw.WriteFullEndElement();
            }

            // final </page>
            if (!string.IsNullOrEmpty(currentPageName))
            {
                xw.WriteFullEndElement();
            }

            // </Resources>
            xw.WriteFullEndElement();

            xw.WriteEndDocument();
            xw.Close();

            this.btnSave.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format(Resources.ErrorSaveDestination, ex.Message), Resources.Error, MessageBoxButtons.OK);

            result = false;
        }

        this.Cursor = Cursors.Default;

        return result;
    }

    /// <summary>
    /// Shows only Pending Translations or all
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckPendingOnlyCheckedChanged(object sender, EventArgs e)
    {
        Settings.Default.ShowPendingOnly = this.checkPendingOnly.Checked;

        Settings.Default.Save();

        if (!string.IsNullOrEmpty(this.tbxSourceTranslationFile.Text)
            && !string.IsNullOrEmpty(this.tbxDestinationTranslationFile.Text))
        {
            this.PopulateTranslations(this.tbxSourceTranslationFile.Text, this.tbxDestinationTranslationFile.Text);
        }
    }

    /// <summary>
    /// Handles the Load event of the TranslateForm control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TranslateForm_Load(object sender, EventArgs e)
    {
        this.checkPendingOnly.Checked = Settings.Default.ShowPendingOnly;

        if (!string.IsNullOrEmpty(Settings.Default.SourceTranslation)
            && !string.IsNullOrEmpty(Settings.Default.DestinationTranslation))
        {
            this.PopulateTranslations(Settings.Default.SourceTranslation, Settings.Default.DestinationTranslation);
        }
    }
}