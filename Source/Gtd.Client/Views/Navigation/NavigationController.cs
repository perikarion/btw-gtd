﻿using System;
using System.Collections.Generic;
using Gtd.Client.Models;
using System.Linq;

namespace Gtd.Client.Views.Navigation
{
    public sealed class NavigationController : 
        IHandle<AppInit>, 
        IHandle<Dumb.ClientModelLoaded>,
        IHandle<Dumb.ThoughtAdded>, 
        IHandle<Dumb.ThoughtRemoved>,
        IHandle<Dumb.ProjectAdded>, 
        IHandle<Dumb.ActionAdded>, 
        IHandle<Dumb.ActionUpdated>,
    IHandle<UI.FilterChanged>
    {
        readonly NavigationView _tree;
        readonly Region _region;
        readonly IPublisher _queue;
        readonly ClientPerspective _view;
        

        bool _loaded;

        NavigationController(Region region, IPublisher queue, ClientPerspective view)
        {
            _tree = new NavigationView(this);
            _region = region;
            _queue = queue;
            _view = view;
        }

        public static NavigationController Wire(Region control, IPublisher queue, ISubscriber bus, ClientPerspective view)
        {
            var adapter  = new NavigationController(control, queue, view);

            bus.Subscribe<AppInit>(adapter);
            bus.Subscribe<Dumb.ThoughtAdded>(adapter);
            bus.Subscribe<Dumb.ThoughtRemoved>(adapter);
            bus.Subscribe<Dumb.ProjectAdded>(adapter);
            bus.Subscribe<Dumb.ActionAdded>(adapter);
            bus.Subscribe<Dumb.ClientModelLoaded>(adapter);
            bus.Subscribe<UI.FilterChanged>(adapter);
            bus.Subscribe<Dumb.ActionUpdated>(adapter);

            return adapter ;
        }

        public void Handle(AppInit message)
        {
            _region.RegisterDock(_tree, "nav-tree");
            _region.SwitchTo("nav-tree");
        }

        

        void ReloadInboxNode()
        {
            _tree.AddOrUpdateNode("inbox",string.Format("Inbox ({0})", _view.ListInbox().Count));
        }

        

        public void Handle(Dumb.ThoughtAdded message)
        {
            Sync(ReloadInboxNode);
        }

        public void Handle(Dumb.ThoughtRemoved message)
        {
            Sync(ReloadInboxNode);
        }

        void Sync(Action act)
        {
            if (_tree.InvokeRequired)
            {
                _tree.Invoke(act);
                return;
            }
            act();
        }

        public void Handle(Dumb.ClientModelLoaded message)
        {
            _loaded = true;

            Sync(LoadNavigation);

        }

        public void Handle(UI.FilterChanged message)
        {
            if (!_loaded) return;
            Sync(LoadNavigation);
        }

        public void Handle(Dumb.ProjectAdded message)
        {
            if (!_loaded)
                return;

            AddOrUpdateProject(_view.GetProjectOrNull(message.ProjectId));
        }

        public void Handle(Dumb.ActionAdded message)
        {
            AddOrUpdateProject(_view.GetProjectOrNull(message.ProjectId));
        }

        public void Handle(Dumb.ActionUpdated message)
        {
            AddOrUpdateProject(_view.GetProjectOrNull(message.ProjectId));
        }

        void AddOrUpdateProject(ProjectView view)
        {
            var actions = _view.CurrentFilter.FilterActions(view);
            var count = _view.CurrentFilter.FormatActionCount(actions.Count());

            var title = string.Format("{0} ({1})", view.Outcome, count);
            _nodes[view.UIKey] = view.ProjectId;
            Sync(() => _tree.AddOrUpdateNode(view.UIKey, title));
        }


        void LoadNavigation()
        {
            _tree.Clear();
            _tree.AddOrUpdateNode("inbox", string.Format("Inbox ({0})", _view.ListInbox().Count));
            foreach (var project in _view.ListProjects())
            {
                AddOrUpdateProject(project);
            }
        }

        readonly IDictionary<string,object> _nodes = new Dictionary<string, object>();

        public void WhenNodeSelected(string tag)
        {
            if (tag == "inbox")
            {
                _queue.Publish(new UI.DisplayInbox());
                return;
            }
            var node = _nodes[tag];

            if (node is ProjectId)
            {
                _queue.Publish(new UI.DisplayProject((ProjectId) node));
            }
        }
    }



    public interface IFormCommand
    {
        
    }
}