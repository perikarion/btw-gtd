﻿using System.Collections.Generic;
using System.Windows.Input;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using Gtd.Client.Core.Models;
using Gtd.Client.Core.Services.Actions;
using Gtd.Client.Core.Services.Inbox;
using Gtd.Client.Core.Services.Projects;

namespace Gtd.Client.Core.ViewModels
{
    public class MakeStuffActionableViewModel : MvxViewModel
    {
        private readonly IMvxMessenger _mvxMessenger;
        private readonly IInboxService _inboxService;
        private readonly IProjectService _projectService;
        private readonly IActionService _actionService;

        private readonly MvxSubscriptionToken _projectsChangedSubToken;

        private ItemOfStuff _itemOfStuff;

        public MakeStuffActionableViewModel(IMvxMessenger mvxMessenger,
                                            IInboxService inboxService,
                                            IProjectService projectService,
                                            IActionService actionService)
        {
            _mvxMessenger = mvxMessenger;
            _inboxService = inboxService;
            _projectService = projectService;
            _actionService = actionService;

            ReloadProjects();

            // subscribe to Projects Changed messages to react to changes in project service
            _projectsChangedSubToken =
                mvxMessenger.Subscribe<ProjectsChangedMessage>(OnProjectsChanged);

            _isSingleActionProject = true;
        }

        public class NavParams
        {
            public string StuffId { get; set; }
        }

        // when we navigate to the a ViewModel, we have a reserved method named called 
        // "Init" and you can expect it to be called with some kind of "Nav" object
        // (which is why we created NavParams above)
        public void Init(NavParams navigationParams)
        {
            // note that the reason we don't pass the entire navigation object across
            // is because this navigation itself needs to be serializable,
            // needs to work across application restarts,
            // and you can't do that with a real object, have to do it with a key (like this Id) instead.
            ItemOfStuff = _inboxService.GetByStuffId(navigationParams.StuffId);

            ActionOutcome = _itemOfStuff.StuffDescription;

            // Set the initial Project Name to whatever the StuffDescription was
            ProjectName = _itemOfStuff.StuffDescription;
        }

        // in this ViewModel we want access to the ItemOfStuff that was 
        // passed in from view via a property
        // will use RaisePropertyChanged to refresh it as needed
        public ItemOfStuff ItemOfStuff
        {
            get { return _itemOfStuff; }
            set { _itemOfStuff = value; RaisePropertyChanged(() => ItemOfStuff); }
        }


        // GTD Project Related

        // list of all Projects in this Trusted System
        private IList<Project> _projectList;
        public IList<Project> ProjectList
        {
            get { return _projectList; }
            set { _projectList = value; RaisePropertyChanged(() => ProjectList); }
        }

        // The GTD Project currently associated with this GTD Action
        private Project _project;
        public Project Project
        {

            get { return _project; }

            set 
            {   _project = value;

                // update the Project Name Property
                // It will raise its own event because we call its public name
                ProjectName = _project.Outcome;

                RaisePropertyChanged(() => Project);
            }
        }

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { _projectName = value; RaisePropertyChanged(() => ProjectName); }
        }

        private MvxCommand _newProjectCommand;
        public ICommand NewProjectCommand
        {
            get
            {
                _newProjectCommand = _newProjectCommand ?? new MvxCommand(DoNewProjectCommand);
                return _newProjectCommand;
            }
        }

        private void DoNewProjectCommand()
        {
            ShowViewModel<CreateNewProjectViewModel>
                (new CreateNewProjectViewModel.NavParams() { StuffDescription = "hi" });
        }

        void OnProjectsChanged(ProjectsChangedMessage message)
        {
            ReloadProjects();
        }

        void ReloadProjects()
        {
            if (_projectService.AllProjects().Count > 0)
            {
                ProjectList = _projectService.AllProjects();
            }
        }

        private bool _isSingleActionProject;
        public bool IsSingleActionProject
        {
            get { return _isSingleActionProject; }
            set { _isSingleActionProject = value; RaisePropertyChanged(() => IsSingleActionProject); }
        }


        // GTD Action Related

        // What’s the next physical/visible action to take on this project?
        private string _actionOutcome;
        public string ActionOutcome
        {
            get { return _actionOutcome; }
            set { _actionOutcome = value; RaisePropertyChanged(() => ActionOutcome); }
        }

        private MvxCommand _saveNewAction;
        public ICommand SaveNewAction
        {
            get
            { 
                _saveNewAction = _saveNewAction ?? new MvxCommand(DoSaveNewActionCommand);
                return _saveNewAction; 
            }
        }

        private void DoSaveNewActionCommand()
        {
            // do action
            // TODO: Need all the properties required for a new action.
            // TODO: Assumes you have already selected a valif projectId
            // TODO: OR you have single action project selected which means
            // TODO: I will create a NEW project/id for you and assign it to this action

            //public string ActionId { get; set; }
            //public string TrustedSystemId { get; set; }
            //public string ProjectId { get; set; }
            //public string Outcome { get; set; }

            // i will generate guid action id
            // i will hard code TSystem for now
            // I will either use the the project id that was selected, or generate
            // I will use the ActionOutcome

            // first, go figure out how to get the ID from the box
        }


        // I want to be able to delete or archive stuff out of inbox if it becomes actionable
        private MvxCommand _trashStuffCommand;
        public ICommand TrashStuffCommand
        {
            get
            {
                _trashStuffCommand = _trashStuffCommand ?? new MvxCommand(DoTrashStuffCommand);
                return _trashStuffCommand;
            }
        }

        private void DoTrashStuffCommand()
        {
            _inboxService.TrashStuff(ItemOfStuff);

            // if you just trashed the stuff you started with then we are done
            // go back to precious screen
            Close(this);
        }


        // TODO: This feature is supported in the domain, but not on the client rigth now
        //private MvxCommand _archiveStuffFromInbox;
        //public ICommand ArchiveStuffFromInbox
        //{
        //    get
        //    {
        //        _archiveStuffFromInbox = _archiveStuffFromInbox ?? new MvxCommand(DoArchiveStuffCommand);
        //        return _archiveStuffFromInbox; 
        //    }
        //}

        //private void DoArchiveStuffCommand()
        //{
        //    // do action
        //}

    }
}
