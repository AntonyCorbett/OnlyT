using System.Windows;

namespace OnlyT.ViewModel.Messages;

internal record class MainWindowSizeChangedMessage(Size PreviousSize, Size NewSize, bool IsShrunk);
