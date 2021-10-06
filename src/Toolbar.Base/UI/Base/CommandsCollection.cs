//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Xarial.CadPlus.CustomToolbar.UI.ViewModels;

namespace Xarial.CadPlus.CustomToolbar.UI.Base
{
    public interface ICommandsCollection
    {
        IList Commands { get; }

        ICommandVM AddNewCommand(int index);
    }

    public class CommandsCollection<TCommandVM> : CompositeCollection, ICommandsCollection, IEnumerable<TCommandVM>
            where TCommandVM : ICommandVM, new()
    {
        public event Action<ICommandVM> NewCommandCreated;

        public event Action<IEnumerable<TCommandVM>> CommandsChanged;

        private readonly List<int> m_UsedIds;

        public ObservableCollection<TCommandVM> Commands { get; }

        IList ICommandsCollection.Commands => Commands;

        public CommandsCollection(IEnumerable<TCommandVM> commands)
        {
            Commands = new ObservableCollection<TCommandVM>(commands);
            Commands.CollectionChanged += OnCommandsCollectionChanged;

            m_UsedIds = Commands.Select(c => c.Command.Id).ToList();

            Add(new CollectionContainer()
            {
                Collection = Commands
            });

            var newCmdPlc = new NewCommandPlaceholderVM();
            newCmdPlc.AddNewCommand += OnAddNewCommand;

            Add(newCmdPlc);
        }

        private void OnAddNewCommand()
        {
            AddNewCommand(Commands.Count);
        }

        public ICommandVM AddNewCommand(int index)
        {
            var newCmd = new TCommandVM();

            var id = GetNextId();
            newCmd.Command.Id = id;

            Commands.Insert(index, newCmd);
            
            AssignDefaultTitle(newCmd);

            NewCommandCreated?.Invoke(newCmd);

            return newCmd;
        }

        private void AssignDefaultTitle(ICommandVM newCmd)
        {
            int index = 0;
            var title = "";

            do
            {
                index++;

                if (newCmd is CommandMacroVM)
                {
                    title = $"Macro {index}";
                }
                else if (newCmd is CommandGroupVM)
                {
                    title = $"Toolbar {index}";
                }
            } while (Commands.FirstOrDefault(
                x => string.Equals(x.Title, title, StringComparison.CurrentCultureIgnoreCase)) != null);

            newCmd.Title = title;
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
            CommandsChanged?.Invoke(Commands);
        }

        public IEnumerator<TCommandVM> GetEnumerator() => Commands.GetEnumerator();
    }
}