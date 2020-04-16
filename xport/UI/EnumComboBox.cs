using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xarial.XTools.Xport.Reflection;
using static Xarial.XTools.Xport.UI.EnumComboBox;

namespace Xarial.XTools.Xport.UI
{
    public class EnumValueToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumVal = value as Enum;

            //TODO: handle the 0 for undefined enum to display empty string instead of 0

            if (enumVal != null)
            {
                var enumType = enumVal.GetType();

                //TODO: this is a simple fix - need to implement more robust solution

                var val = enumVal.ToString();
                var vals = val.Split(',')
                    .Select(v => (Enum)Enum.Parse(enumType, v.Trim()))
                    .Select(e => EnumComboBoxItem.GetTitle(e));

                return string.Join(", ", vals.ToArray());
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Item { get; set; }
        public DataTemplate Header { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var elem = container as FrameworkElement;

            if (elem?.TemplatedParent is EnumComboBox)
            {
                return Header;
            }
            else
            {
                return Item;
            }
        }
    }

    public class EnumItemTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumItemType_e)
            {
                switch ((EnumItemType_e)value)
                {
                    case EnumItemType_e.Default:
                        return Brushes.Black;

                    case EnumItemType_e.Combined:
                        return Brushes.Blue;

                    case EnumItemType_e.None:
                        return Brushes.Gray;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyChanged([CallerMemberName] string prpName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prpName));
        }
    }

    public class EnumComboBox : ComboBox
    {
        public enum EnumItemType_e
        {
            Default,
            Combined,
            None
        }

        public class EnumComboBoxItem : NotifyPropertyChanged
        {
            internal static string GetTitle(Enum value)
            {
                string title = "";

                if (value != null)
                {
                    if (!value.TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName))
                    {
                        title = value.ToString();
                    }
                }

                return title;
            }

            internal static string GetDescription(Enum value)
            {
                string title = "";

                if (value != null)
                {
                    if (!value.TryGetAttribute<DescriptionAttribute>(a => title = a.Description))
                    {
                        title = GetTitle(value);

                        if (string.IsNullOrEmpty(title))
                        {
                            title = value.ToString();
                        }
                    }
                }

                return title;
            }

            private readonly EnumComboBox m_Parent;
            private readonly Enum m_Value;
            private readonly Enum[] m_AffectedFlags;

            internal EnumComboBoxItem(EnumComboBox parent, Enum value, Enum[] affectedFlags)
            {
                m_Parent = parent;
                m_Parent.ValueChanged += OnValueChanged;
                m_Value = value;
                m_AffectedFlags = affectedFlags;

                if (m_AffectedFlags.Length > 1)
                {
                    Type = EnumItemType_e.Combined;
                }
                else if (m_AffectedFlags.Length == 0)
                {
                    Type = EnumItemType_e.None;
                }
                else
                {
                    Type = EnumItemType_e.Default;
                }

                Title = GetDescription(m_Value);

                if (!value.TryGetAttribute<DescriptionAttribute>(a => Description = a.Description))
                {
                    Description = m_Value.ToString();
                }
            }

            public EnumItemType_e Type { get; private set; }

            public bool IsSelected
            {
                get
                {
                    if (m_Parent.Value != null)
                    {
                        if (Type == EnumItemType_e.None)
                        {
                            return IsNone(m_Parent.Value);
                        }
                        else
                        {
                            return m_Parent.Value.HasFlag(m_Value);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                set
                {
                    if (Type == EnumItemType_e.None)
                    {
                        m_Parent.Value = (Enum)Enum.ToObject(m_Value.GetType(), 0);
                    }
                    else
                    {
                        int val = Convert.ToInt32(m_Parent.Value);

                        if (value)
                        {
                            foreach (var flag in m_AffectedFlags)
                            {
                                if (!m_Parent.Value.HasFlag(flag))
                                {
                                    val += Convert.ToInt32(flag);
                                }
                            }
                        }
                        else
                        {
                            foreach (var flag in m_AffectedFlags)
                            {
                                if (m_Parent.Value.HasFlag(flag))
                                {
                                    val -= Convert.ToInt32(flag);
                                }
                            }
                        }

                        m_Parent.Value = (Enum)Enum.ToObject(m_Value.GetType(), val);
                    }

                    NotifyChanged();
                }
            }

            public string Title { get; private set; }
            public string Description { get; private set; }

            private void OnValueChanged(Enum value)
            {
                NotifyChanged(nameof(IsSelected));
            }

            private bool IsNone(Enum val)
            {
                return Convert.ToInt32(val) == 0;
            }

            public override string ToString()
            {
                return m_Value.ToString();
            }
        }

        internal event Action<Enum> ValueChanged;

        private Type m_CurBoundType;
        private Enum[] m_CurFlags;

        static EnumComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumComboBox),
                new FrameworkPropertyMetadata(typeof(EnumComboBox)));
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(Enum),
            typeof(EnumComboBox), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public Enum Value
        {
            get { return (Enum)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cmb = d as EnumComboBox;

            var val = e.NewValue as Enum;

            if (val != null)
            {
                var enumType = val.GetType();

                if (enumType != cmb.m_CurBoundType)
                {
                    cmb.m_CurFlags = EnumHelper.GetFlags(enumType);

                    cmb.Items.Clear();

                    cmb.m_CurBoundType = enumType;

                    var items = Enum.GetValues(enumType);

                    foreach (Enum item in items)
                    {
                        cmb.Items.Add(new EnumComboBoxItem(cmb, item,
                            cmb.m_CurFlags.Where(f => item.HasFlag(f)).ToArray()));
                    }

                    UpdateHeader(cmb);
                }
            }

            cmb.ValueChanged?.Invoke(val);
        }

        private static void UpdateHeader(EnumComboBox cmb)
        {
            if (cmb.Items.Count > 0)
            {
                cmb.SelectedIndex = 0;
            }
        }
    }
}
