<Query Kind="Program">
  <GACReference>Microsoft.TeamFoundation.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</GACReference>
  <GACReference>Microsoft.TeamFoundation.VersionControl.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</GACReference>
  <GACReference>Microsoft.TeamFoundation.VersionControl.Common, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</GACReference>
  <GACReference>Microsoft.TeamFoundation.WorkItemTracking.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</GACReference>
  <Namespace>Microsoft.TeamFoundation.Client</Namespace>
  <Namespace>Microsoft.TeamFoundation.VersionControl.Client</Namespace>
  <Namespace>Microsoft.TeamFoundation.WorkItemTracking.Client</Namespace>
</Query>

void Main()
{
    ILogger logger = new DebugLogger();
    
    TfsProject tfsProject = new TfsProject(
        @"http://ap-dcr-tfsapp01:8080/tfs/MSched",
        @"MSched.Development",
        @"C:\Temp\MSched.Development",
        logger);

    tfsProject.Dump("foo");
}

public enum GemcomWorkItemType
{
    Bug,
    ProductBacklogItem,
    Support
}

public interface ITfsWorkItem
{
    string Title { get; set; }

    string AreaPath { get; set; }

    string State { get; set; }

    void SetFieldValue(string fieldId, object fieldValue);

    void SetFieldValue(string fieldId, object fieldValue, string globalListName);

    object GetFieldValue(string fieldId);

    void Save();

    bool IsValid(List<string> log);

    ArrayList Validate();

    ITfsAttachment SetAttachment(string path);

    void SetRelatedLink(ITfsWorkItem relatedWorkItem, string comment);

    void SetRelatedLink(ITfsWorkItem[] relatedWorkItems, string comment);

    void SetParentChildLink(ITfsWorkItem parentWorkItem, ITfsWorkItem childWorkItem, string comment);

    void SetParentChildLink(ITfsWorkItem parentWorkItem, ITfsWorkItem[] childWorkItems, string comment);

    int Id { get; }

    GemcomWorkItemType GemcomWorkItemType { get; set; }

    string Description { get; set; }
}
    
public interface ITfsAttachment
{
    Uri Uri { get; }
}

public interface ITfsProject
{
    /// <summary>
    /// Connect to the TFS project collection
    /// </summary>
    void Connect(string projectCollectionUri, string projectName);

    ITfsWorkItem CreateNewWorkItem(GemcomWorkItemType gemcomWorkItemType);

    ITfsWorkItem FindWorkItem(int id);
}

public class TfsAttachment : ITfsAttachment
{
    private Attachment _attachment;

    public TfsAttachment(Attachment attachment)
    {
        _attachment = attachment;
    }

    public Uri Uri
    {
        get
        {
            return _attachment.Uri;
        }
    }
}

public class TfsWorkItem : ITfsWorkItem
{
    private WorkItem _workItem;
    private WorkItemStore _store;
    private GemcomWorkItemType _gemcomWorkItemType;
    static private XmlDocument _globalLists;
    static private bool _globalListsChanged = false;

    public TfsWorkItem(WorkItem workItem, WorkItemStore store, GemcomWorkItemType gemcomWorkItemType)
    {
        _workItem = workItem;
        _store = store;
        _gemcomWorkItemType = gemcomWorkItemType;
    }

    #region ITfsWorkItem Members

    public string Title
    {
        get
        {
            return _workItem.Title;
        }
        set
        {
            _workItem.Title = value;
        }
    }

    public string AreaPath
    {
        get
        {
            return _workItem.AreaPath;
        }
        set
        {
            _workItem.AreaPath = value;
        }
    }

    public string State
    {
        get
        {
            return _workItem.State;
        }
        set
        {
            _workItem.State = value;
        }
    }

    public string Description
    {
        get
        {
            return _workItem.Description;
        }
        set
        {
            _workItem.Description = value;
        }
    }

    public object GetFieldValue(string fieldId)
    {
        return _workItem.Fields[fieldId].Value;
    }

    public void SetFieldValue(string fieldId, object fieldValue, string globalListName)
    {
        UpdateGlobalList(globalListName, fieldValue.ToString());
        SetFieldValue(fieldId, fieldValue);
    }

    private void UpdateGlobalList(string globalListName, string listValue)
    {
        XmlDocument xml = GetGlobalLists();
        XmlElement globalList = xml.SelectSingleNode("//GLOBALLIST[@name='" + globalListName + "']") as XmlElement;
        if (globalList == null)
        {
            globalList = xml.CreateElement("GLOBALLIST");
            globalList.SetAttribute("name", globalListName);
        }
        HashSet<string> tfsCustomerNames = new HashSet<string>(globalList.SelectNodes("LISTITEM[@value]").Cast<XmlElement>().Select(s => s.GetAttribute("value")), StringComparer.OrdinalIgnoreCase);

        bool listValueAdded = false;
        if (!tfsCustomerNames.Contains(listValue))
        {
            XmlElement listItem = xml.CreateElement("LISTITEM");
            listItem.SetAttribute("value", listValue);
            globalList.AppendChild(listItem);
            listValueAdded = true;
        }

        if (listValueAdded)
        {
            SetGlobalLists(xml);
        }
    }

    private void SetGlobalLists(XmlDocument xml)
    {
        _globalLists = xml;
        _globalListsChanged = true;
        try
        {
            _store.ImportGlobalLists(_globalLists.DocumentElement);
        }
        catch (Exception e)
        {
            Debug.WriteLine(string.Format("ImportGlobalLists failed because: {0} \n {1}", e.Message, e.StackTrace));
        }
    }

    private XmlDocument GetGlobalLists()
    {
        if (_globalLists == null)
        {
            _globalLists = _store.ExportGlobalLists();
        }
        return _globalLists;
    }

    static public void SaveGlobalLists(string savePath)
    {
        if ((_globalLists != null) && _globalListsChanged)
        {
            // Save the globallists if they have changed.
            string baseFileName = Path.Combine(savePath, "GlobalLists");

            using (StreamWriter writer = new StreamWriter(baseFileName + ".xml"))
            {
                writer.Write(_globalLists.OuterXml);
            }
        }
    }

    public void SetFieldValue(string fieldId, object fieldValue)
    {
        _workItem.Fields[fieldId].Value = fieldValue;
    }

    public ITfsAttachment SetAttachment(string path)
    {
        Attachment attachment = new Attachment(path);
        _workItem.Attachments.Add(attachment);

        return new TfsAttachment(attachment);
    }

    public void SetRelatedLink(ITfsWorkItem relatedWorkItem, string comment)
    {
        this.SetRelatedLink(new[] { relatedWorkItem }, comment);
    }

    public void SetRelatedLink(ITfsWorkItem[] relatedWorkItems, string comment)
    {
        WorkItemLinkTypeEnd linkTypeEnd = _store.WorkItemLinkTypes.LinkTypeEnds["Related"];
        foreach (var item in relatedWorkItems)
        {
            _workItem.Links.Add(new RelatedLink(linkTypeEnd, item.Id) { Comment = comment });
        }
    }

    public void SetParentChildLink(ITfsWorkItem parentWorkItem, ITfsWorkItem childWorkItem, string comment)
    {
        this.SetParentChildLink(parentWorkItem, new[] { childWorkItem }, comment);
    }

    public void SetParentChildLink(ITfsWorkItem parentWorkItem, ITfsWorkItem[] childWorkItems, string comment)
    {
        // todo
    }

    public bool IsValid(List<string> log)
    {
        ArrayList validation = Validate();

        if (validation.Count == 0)
        {
            return true;
        }

        foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.Field validationField in validation)
        {
            log.Add(string.Format("Name: {0}, ReferenceName: {1}, Status: {2}, Value: {3}", validationField.Name, validationField.ReferenceName, validationField.Status.ToString(), GetFieldValue(validationField.ReferenceName).ToString()));
        }

        return false;
    }

    public int Id
    {
        get
        {
            return _workItem.Id;
        }
    }

    public GemcomWorkItemType GemcomWorkItemType
    {
        get
        {
            return _gemcomWorkItemType;
        }
        set
        {
            _gemcomWorkItemType = value;
        }
    }

    public void Save()
    {
        _workItem.Save();
    }

    public ArrayList Validate()
    {
        return _workItem.Validate();
    }

    #endregion
}

public class TfsProject : ITfsProject
{
    private WorkItemType _supportWorkItemType;

    private WorkItemType _bugWorkItemType;

    private WorkItemType _productBacklogItemWorkItemType;

    private VersionControlServer versionControlServer;
    private Workspace workspace;

    private readonly string workspaceFolder;

    private Uri uri;
    private readonly ILogger logger;

    public TfsProject(string projectCollectionUri, string projectName, string workspaceFolder, ILogger logger)
    {
        this.logger = logger;
        this.uri = new Uri(projectCollectionUri);

        this.workspaceFolder = workspaceFolder;
        Connect(projectCollectionUri, projectName);
    }

    #region ITeamFoundationServer Members

    public void Connect(string projectCollectionUri, string projectName)
    {
        // Open the TFS project collection
        TfsTeamProjectCollection projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(uri);
        WorkItemStore store = (WorkItemStore)projectCollection.GetService(typeof (WorkItemStore));

        // Validate the project name
        Project project = store.Projects[projectName];
        if (project == null)
        {
            throw new ArgumentException(
                string.Format(
                    "TFS Project {0} does not existing in Project Collection {1}", projectName, projectCollectionUri));
        }

        InitVersionControl(projectCollection, projectName);
    }

    private void InitVersionControl(TfsTeamProjectCollection projectCollection, string project)
    {
        this.versionControlServer = projectCollection.GetService<VersionControlServer>();

        string serverFolder = string.Format("$/{0}", project);

        workspace = versionControlServer.TryGetWorkspace(workspaceFolder);

        string userName = Environment.UserName;
        string machineName = Environment.MachineName;
        string workspaceName = "Tfs2Hg_MattHalstead";

//            string oldWorkspaceName = string.Format("Tfs2Hg_Matt_{0}", shortBranch);
        //Workspace[] workspaces = this.versionControlServer.QueryWorkspaces(null, userName, machineName).ToArray();
        //foreach (var ws in workspaces)
        //{
        //    this.versionControlServer.DeleteWorkspace(ws.Name, userName);

        //}

        this.workspace = this.versionControlServer.QueryWorkspaces(workspaceName, userName, machineName).FirstOrDefault();
        if (this.workspace == null)
        {
            this.workspace = this.versionControlServer.CreateWorkspace(workspaceName, userName, "Created by Matt while exporting from TFS", new[] { new WorkingFolder(serverFolder, this.workspaceFolder) });

            // HACK, dont import the huge test results into HG
            //this.workspace.Cloak("$\\MSched.Development\\Main\\Tools\\AutomatedTesting\\TestResults-BSF");
        }
    }

    public void EnsureCloaked(string fullBranch, string cloakServerFolder)
    {
        string serverFolder = string.Format("{0}\\{1}", fullBranch.Replace("/", "\\"), cloakServerFolder);
        if (this.workspace != null)
        {
            this.workspace.Cloak(serverFolder);
        }
    }

    public ITfsWorkItem CreateNewWorkItem(GemcomWorkItemType gemcomWorkItemType)
    {
        switch (gemcomWorkItemType)
        {
            case GemcomWorkItemType.Bug:
                return new TfsWorkItem(_bugWorkItemType.NewWorkItem(), _bugWorkItemType.Store, gemcomWorkItemType);

            case GemcomWorkItemType.ProductBacklogItem:
                return new TfsWorkItem(
                    _productBacklogItemWorkItemType.NewWorkItem(), _productBacklogItemWorkItemType.Store, gemcomWorkItemType);

            case GemcomWorkItemType.Support:
                return new TfsWorkItem(_supportWorkItemType.NewWorkItem(), _supportWorkItemType.Store, gemcomWorkItemType);

            default:
                return new TfsWorkItem(
                    _supportWorkItemType.NewWorkItem(), _supportWorkItemType.Store, GemcomWorkItemType.Support);
        }
    }

    public ITfsWorkItem FindWorkItem(int id)
    {
        try
        {
            var item = this._supportWorkItemType.Store.GetWorkItem(id);
            if (item != null)
            {
                return new TfsWorkItem(item, _supportWorkItemType.Store, GemcomWorkItemType.Support);
            }
        }
        catch (Exception)
        {
        }

        logger.Log("Error: Could not find work item {0}.", id);
        return null;
    }

    public IEnumerable<Changeset> GetChangesetHistory(string branch, VersionSpec fromVersionSpec, VersionSpec toVersionSpec, bool getChanges)
    {
        return versionControlServer.QueryHistory(branch, VersionSpec.Latest, 0, RecursionType.Full, null, fromVersionSpec, toVersionSpec, Int32.MaxValue, getChanges, false)
            .OfType<Changeset>()
            .OrderBy(x => x.ChangesetId);
    }

    public ChangesetVersionSpec GetChangeset(Changeset changeset)
    {
        if (workspace == null)
        {
            throw new InvalidOperationException("Cannot retrieve a changeset before initialising workspace.");
        }

        ChangesetVersionSpec changesetVersionSpec = new ChangesetVersionSpec(changeset.ChangesetId);
        workspace.Get(changesetVersionSpec, GetOptions.Overwrite);

        return changesetVersionSpec;
    }

    /// <summary>
    /// Returns pair of [branch, parent branch]
    /// </summary>
    /// <returns></returns>
    public List<Tuple<string, string>> FindAllBranches()
    {
        logger.Log("TFS: Finding all branches...");
        BranchObject[] branchObjects = this.versionControlServer.QueryRootBranchObjects(RecursionType.OneLevel);

        var branches = new List<Tuple<string, string>>();

        foreach (var bo in branchObjects)
        {
            this.FindAllBranches(branches, bo);
        }

        return branches;
    }

    private void FindAllBranches(List<Tuple<string, string>> branches, BranchObject bo)
    {
        BranchObject[] childBos = this.versionControlServer.QueryBranchObjects(bo.Properties.RootItem, RecursionType.OneLevel);

        string branch = bo.Properties.RootItem.Item;
        string parentBranch = bo.Properties.ParentBranch != null ? bo.Properties.ParentBranch.Item : string.Empty;
        
        branches.Add(Tuple.Create(branch, parentBranch));
        logger.Log("  Found branch '{0}' with parent '{1}'", branch, parentBranch ?? "null");

        foreach (BranchObject child in childBos)
        {
            if (child.Properties.RootItem.Item == branch)
            {
                continue;
            }

            this.FindAllBranches(branches, child);
        }
    }

    #endregion
}

public interface ILogger
{
    void Log(string format, params object[] args);
}

public class DebugLogger : ILogger
{
    public void Log(string format, params object[] args)
    {
        Console.WriteLine(format, args);
        Debug.WriteLine(format, args);
    }
}