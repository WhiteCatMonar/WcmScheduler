using System.Windows;

namespace MainApplication.Views
{
    /// <summary>
    /// XAML内でDataContextを別のスコープへ渡すためのプロキシオブジェクト。
    /// 
    /// Freezableを継承することで、VisualTreeに属さなくても
    /// データバインディングが有効になるというWPFの特性を利用している。
    /// 
    /// 主に以下のような用途で使用される：
    /// ・StaticResource/ResourceDictionary内からDataContextを参照したい
    /// ・Bindingのスコープ制限により直接参照できないViewModelを渡したい
    /// </summary>
    public class BindingProxy : Freezable
    {
        /* ---------------------------------------------------------
         * Freezableの実装
         * --------------------------------------------------------- */

        /// <summary>
        /// Freezableの派生クラスに必須の実装。
        /// 新しいBindingProxyインスタンスを生成する。
        /// </summary>
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        /* ---------------------------------------------------------
         * Dataプロパティ(XAMLから任意のオブジェクトを渡す)
         * --------------------------------------------------------- */

        /// <summary>
        /// プロキシとして渡したい任意のデータ(通常はViewModel)。
        /// XAML内でStaticResourceとして参照される。
        /// </summary>
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary>
        /// DataのDependencyProperty定義。
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy));
    }
}

/* --- End of file --- */
