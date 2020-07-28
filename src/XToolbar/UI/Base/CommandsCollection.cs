//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Xarial.CadPlus.XToolbar.UI.ViewModels;

namespace Xarial.CadPlus.XToolbar.UI.Base
{
    public interface ICommandsCollection
    {
        IList Commands { get; }

        ICommandVM AddNewCommand(int index);
    }

    public class CommandsCollection<TCommandVM> : CompositeCollection, ICommandsCollection
            where TCommandVM : ICommandVM, new()
    {
        public event Action<ICommandVM> NewCommandCreated;

        public event Action<IEnumerable<TCommandVM>> CommandsChanged;

        private readonly ObservableCollection<TCommandVM> m_Commands;

        private readonly List<int> m_UsedIds;

        public ObservableCollection<TCommandVM> Commands
        {
            get
            {
                return m_Commands;
            }
        }

        IList ICommandsCollection.Commands
        {
            get
            {
                return Commands;
            }
        }

        public CommandsCollection(IEnumerable<TCommandVM> commands)
        {
            m_Commands = new ObservableCollection<TCommandVM>(commands);
            m_Commands.CollectionChanged += OnCommandsCollectionChanged;

            m_UsedIds = m_Commands.Select(c => c.Command.Id).ToList();

            Add(new CollectionContainer()
            {
                Collection = m_Commands
            });

            var newCmdPlc = new NewCommandPlaceholderVM();
            newCmdPlc.AddNewCommand += OnAddNewCommand;

            Add(newCmdPlc);
        }

        private void OnAddNewCommand()
        {
            AddNewCommand(m_Commands.Count);
        }

        public ICommandVM AddNewCommand(int index)
        {
            var newCmd = new TCommandVM();

            newCmd.Command.Id = GetNextId();

            m_Commands.Insert(index, newCmd);

            NewCommandCreated?.Invoke(newCmd);

            return newCmd;
        }
        
        private int GetNextId()
        {
            int id = 1;

            while (m_UsedIds.Contains(id)) 
            {
                id++;
            }

            m_UsedIds.Add(id);

            return id;
        }

        private void OnCommandsCollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommandsChanged?.Invoke(m_Commands);
        }
    }
}