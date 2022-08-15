using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.ViewModels
{
	public class BatchJobItemOperationDefinitionVM
	{
		public ImageSource Icon => Definition.Icon;
		public string Name => Definition.Name;

		public IBatchJobItemOperationDefinition Definition { get; }

		public BatchJobItemOperationDefinitionVM(IBatchJobItemOperationDefinition definition)
		{
			Definition = definition;
		}
	}
}
