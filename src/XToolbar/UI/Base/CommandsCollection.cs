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

        private readonly List<int> m_AvailableIds;

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

            m_AvailableIds = new List<int>(GetAvailableIds(m_Commands));

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

        private IEnumerable<int> GetAvailableIds(IEnumerable<TCommandVM> commands)
        {
            var availableIds = new List<int>();

            var usedIds = commands.Select(c => c.Command.Id).ToList();

            if (usedIds.Any())
            {
                for (int i = 1; i < usedIds.Max(); i++)
                {
                    if (!usedIds.Contains(i))
                    {
                        availableIds.Add(i);
                    }
                }
            }

            return availableIds;
        }

        private int GetNextId()
        {
            if (m_AvailableIds.Any())
            {
                var id = m_AvailableIds.First();
                m_AvailableIds.RemoveAt(0);
                return id;
            }
            else
            {
                if (m_Commands.Any())
                {
                    return m_Commands.Max(c => c.Command.Id) + 1;
                }
                else
                {
                    return 1;
                }
            }
        }

        private void OnCommandsCollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommandsChanged?.Invoke(m_Commands);
        }
    }
}