using System;
using System.Windows;
using System.Windows.Input;
using AkashaNavigator.Helpers;
using AkashaNavigator.ViewModels.Dialogs;

namespace AkashaNavigator.Views.Dialogs
{
    /// <summary>
    /// Profile选择器对话框 - 用于将插件添加到选定的Profile
    /// </summary>
    public partial class ProfileSelectorDialog : AnimatedWindow
    {
        private readonly ProfileSelectorDialogViewModel _viewModel;

        /// <summary>
        /// DI容器注入的构造函数
        /// </summary>
        public ProfileSelectorDialog(ProfileSelectorDialogViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();

            DataContext = _viewModel;

            // 订阅关闭请求事件
            _viewModel.RequestClose += OnRequestClose;
        }

        /// <summary>
        /// 标题栏拖动
        /// </summary>
        private new void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.TitleBar_MouseLeftButtonDown(sender, e);
        }

        /// <summary>
        /// 处理 ViewModel 的关闭请求
        /// </summary>
        private void OnRequestClose(object? sender, bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
