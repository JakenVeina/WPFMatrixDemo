using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WPFMatrixDemo
{
    /// <summary>
    /// Combines the logical structure of <see cref="StackPanel"/> with the arrangement logic of <see cref="Grid"/>.
    /// Children are arranged in a vertical or horizontal stack, and are stretched to fill the available space within the panel.
    /// </summary>
    public class StretchPanel : Panel
    {
        /**********************************************************************/
        #region Attached Properties

        /// <summary>
        /// <para>
        /// Identifies the <see cref="StretchPanel"/>.Span attached property.
        /// </para>
        /// <para>
        /// This property defines how much space is alloted to each child within a parent <see cref="StretchPanel"/>,
        /// relative to the total available space, and the space consumed by other children.
        /// </para>
        /// <para>
        /// For example, if two children are present with Span values 1.0 and 2.0, the first child is allotted 1/3 of the total space,
        /// and the second child is allotted 2/3 of the total space.
        /// </para>
        /// <para>
        /// The default value is 1.0.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty SpanProperty
             = DependencyProperty.RegisterAttached(nameof(SpanProperty).Replace("Property", ""),
                                                   typeof(double),
                                                   typeof(StretchPanel),
                                                   new FrameworkPropertyMetadata(1.0,
                                                       FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                                                       FrameworkPropertyMetadataOptions.AffectsParentArrange),
                                                   ValidateSpan);

        /// <summary>
        /// Retrieves the current value of the <see cref="StretchPanel"/>.Span attached property, for a given <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> whose <see cref="StretchPanel"/>.Span value is to be retrieved.</param>
        /// <returns>The current <see cref="StretchPanel"/>.Span value of <paramref name="element"/>.</returns>
        public static double GetSpan(UIElement element)
            => (double)element.GetValue(SpanProperty);

        /// <summary>
        /// Sets the current value of the <see cref="StretchPanel"/>.Span attached property, for a given <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> whose <see cref="StretchPanel"/>.Span value is to be set.</param>
        /// <param name="value">The value to assign to the <see cref="StretchPanel"/>.Span property of <paramref name="element"/>.</param>
        public static void SetSpan(UIElement element, double value)
            => element.SetValue(SpanProperty, value);

        #endregion Attached Properties

        /**********************************************************************/
        #region Properties

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty
             = DependencyProperty.Register(nameof(Orientation),
                                           typeof(Orientation),
                                           typeof(StretchPanel),
                                           new FrameworkPropertyMetadata(Orientation.Vertical,
                                                       FrameworkPropertyMetadataOptions.AffectsRender),
                                           ValidateOrientation);

        /// <summary>
        /// Defines the orientation of children to be laid out by this <see cref="StretchPanel"/>;
        /// either in a vertical stack or a horizontal stack.
        /// The default value is <see cref="Orientation.Vertical"/>.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion Properties

        /**********************************************************************/
        #region FrameworkElement Overrides

        /// <summary>
        /// See <see cref="FrameworkElement.MeasureOverride(Size)"/>.
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            var desiredWidth = 0.0;
            var desiredHeight = 0.0;

            EnumerateChildenAndCalculateSizes(availableSize)
                .Do(x => x.ChildElement.Measure(x.CalculatedSize))
                .Do(x => desiredWidth = (Orientation == Orientation.Vertical) ?
                                        System.Math.Max(desiredWidth, x.ChildElement.DesiredSize.Width) :
                                        (desiredWidth + x.ChildElement.DesiredSize.Width))
                .Do(x => desiredHeight = (Orientation == Orientation.Vertical) ?
                                         (desiredHeight + x.ChildElement.DesiredSize.Height) :
                                         System.Math.Max(desiredHeight, x.ChildElement.DesiredSize.Height))
                .ForEach();

            return new Size(desiredWidth, desiredHeight);
        }

        /// <summary>
        /// See <see cref="FrameworkElement.ArrangeOverride(Size)"/>.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var childPosition = new Point();

            EnumerateChildenAndCalculateSizes(finalSize)
                .Do(x => x.ChildElement.Arrange(new Rect(childPosition, x.CalculatedSize)))
                .Do(x => childPosition += (Orientation == Orientation.Vertical) ?
                                          new Vector(0, x.CalculatedSize.Height) :
                                          new Vector(x.CalculatedSize.Width, 0))
                .ForEach();

            return finalSize;
        }

        #endregion FrameworkElement Overrides

        /**********************************************************************/
        #region Private Methods

        private static bool ValidateSpan(object value)
            => (double)value >= 0.0;

        private static bool ValidateOrientation(object value)
            => Enum.IsDefined(typeof(Orientation), value);

        private IEnumerable<ChildElementWithCalculatedSize> EnumerateChildenAndCalculateSizes(Size containerSize)
        {
            var totalSpan = 0.0;

            return InternalChildren.Cast<UIElement>()
                                   .Select(x => new { ChildElement = x, Span = GetSpan(x) })
                                   .Do(x => totalSpan += x.Span)
                                   .ToArray()
                                   .Select(x => new { ChildElement = x.ChildElement, WeightedSpan = x.Span / totalSpan })
                                   .Select(x => new ChildElementWithCalculatedSize()
                                   {
                                       ChildElement = x.ChildElement,
                                       CalculatedSize = (Orientation == Orientation.Vertical) ?
                                                        new Size(containerSize.Width, (containerSize.Height * x.WeightedSpan)) :
                                                        new Size((containerSize.Width * x.WeightedSpan), containerSize.Height)
                                   });
        }

        #endregion Private Methods

        /**********************************************************************/
        #region Private Types

        private struct ChildElementWithCalculatedSize
        {
            public UIElement ChildElement;
            public Size CalculatedSize;
        }

        #endregion Private Types
    }
}
