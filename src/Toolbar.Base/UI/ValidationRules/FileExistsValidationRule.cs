//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xarial.CadPlus.CustomToolbar;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.Toolbar.UI.ValidationRules
{
    public class FileExistsValidationRuleParameters : DependencyObject
    {
        public static readonly DependencyProperty WorkingDirectoryProperty =
            DependencyProperty.Register(
            nameof(WorkingDirectory), typeof(string),
            typeof(FileExistsValidationRuleParameters),
            new PropertyMetadata(default(string), OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var parameters = (FileExistsValidationRuleParameters)d;
            parameters.m_Binding?.UpdateSource();
        }

        public string WorkingDirectory
        {
            get { return (string)GetValue(WorkingDirectoryProperty); }
            set { SetValue(WorkingDirectoryProperty, value); }
        }

        private BindingExpressionBase m_Binding;

        internal void SetBindingExpression(BindingExpressionBase binding) 
        {
            m_Binding = binding;
        }
    }

    public class FileExistsValidationRule : ValidationRule
    {
        public FileExistsValidationRuleParameters Parameters { get; set; }

        public bool AllowEmptyValues { get; set; } = false;

        private readonly IFilePathResolver m_FilePathResolver;

        public FileExistsValidationRule() : this(ToolbarModule.Resolve<IFilePathResolver>())
        {
        }

        public FileExistsValidationRule(IFilePathResolver filePathResolver)
        {
            m_FilePathResolver = filePathResolver;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
        {
            Parameters?.SetBindingExpression(owner);
            return base.Validate(value, cultureInfo, owner);
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(value as string))
                {
                    var filePath = (string)value;

                    if (Parameters != null)
                    {
                        filePath = m_FilePathResolver.Resolve(filePath, Parameters.WorkingDirectory);
                    }

                    if (!File.Exists(filePath))
                    {
                        return new ValidationResult(false, "File does not exist");
                    }
                    else
                    {
                        return ValidationResult.ValidResult;
                    }
                }
                else
                {
                    if (AllowEmptyValues)
                    {
                        return ValidationResult.ValidResult;
                    }
                    else
                    {
                        return new ValidationResult(false, "Path is not specified");
                    }
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.ParseUserError("Invalid path"));
            }
        }
    }
}
