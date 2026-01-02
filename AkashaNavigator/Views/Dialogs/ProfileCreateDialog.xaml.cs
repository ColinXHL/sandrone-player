using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AkashaNavigator.Helpers;
using AkashaNavigator.ViewModels.Dialogs;

namespace AkashaNavigator.Views.Dialogs
{
    /// <summary>
    /// Profile 创建对话框
    /// </summary>
    public partial class ProfileCreateDialog : AnimatedWindow
    {
        #region Properties

        /// <summary>
        /// 是否确认创建
        /// </summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>
        /// 创建的 Profile ID
        /// </summary>
        public string? ProfileId { get; private set; }

        #endregion

        #region Fields

        private readonly ProfileCreateDialogViewModel _viewModel;

        #endregion

        #region Constructor

        public ProfileCreateDialog(ProfileCreateDialogViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            InitializeComponent();

            DataContext = _viewModel;

            // 初始化图标选择器（UI 逻辑保留在 Code-behind）
            InitializeIconSelector();

            // 订阅 ViewModel 的关闭请求
            _viewModel.RequestClose += OnRequestClose;
        }

        #endregion

        #region Icon Selector (UI 逻辑)

        /// <summary>
        /// 初始化图标选择器（UI 逻辑保留在 Code-behind）
        /// </summary>
        private void InitializeIconSelector()
        {
            bool isFirst = true;

            foreach (var icon in _viewModel.AvailableIcons)
            {
                var radioButton = new RadioButton
                {
                    Content = icon,
                    FontSize = 16,
                    GroupName = "IconGroup",
                    Tag = icon,
                    IsChecked = isFirst
                };
                radioButton.Style = (Style)FindResource("IconButtonStyle");
                radioButton.Checked += IconButton_Checked;

                IconPanel.Children.Add(radioButton);

                if (isFirst)
                {
                    isFirst = false;
                }
            }
        }

        private void IconButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string icon)
            {
                _viewModel.SelectedIcon = icon;
            }
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// 标题栏拖动
        /// </summary>
        private new void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.TitleBar_MouseLeftButtonDown(sender, e);
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CloseCommand.Execute(null);
        }

        /// <summary>
        /// 名称输入框变化时更新占位符可见性
        /// </summary>
        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            NamePlaceholder.Visibility = string.IsNullOrEmpty(TxtName.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 处理 ViewModel 的关闭请求
        /// </summary>
        private void OnRequestClose(object? sender, bool? dialogResult)
        {
            IsConfirmed = dialogResult == true;
            ProfileId = _viewModel.CreatedProfileId;
            DialogResult = dialogResult;
            CloseWithAnimation();
        }

        #endregion
    }
}
