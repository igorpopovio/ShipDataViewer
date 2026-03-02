using System.Windows;
using System.Windows.Media;

namespace ShipDataViewer.Shell;

public partial class Gadget
{
	public SolidColorBrush AccentBrush
	{
		get { return (SolidColorBrush)GetValue(AccentBrushProperty); }
		set { SetValue(AccentBrushProperty, value); }
	}

	public static readonly DependencyProperty AccentBrushProperty =
		DependencyProperty.Register(nameof(AccentBrush), typeof(SolidColorBrush), typeof(Gadget), new PropertyMetadata(Brushes.Salmon));

	public string Label
	{
		get { return (string)GetValue(LabelProperty); }
		set { SetValue(LabelProperty, value); }
	}

	public static readonly DependencyProperty LabelProperty =
		DependencyProperty.Register(nameof(Label), typeof(string), typeof(Gadget), new PropertyMetadata(""));

	public object Value
	{
		get { return (object)GetValue(ValueProperty); }
		set { SetValue(ValueProperty, value); }
	}

	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(nameof(Value), typeof(object), typeof(Gadget), new PropertyMetadata("N/A"));

	public Gadget()
	{
		InitializeComponent();
	}
}
