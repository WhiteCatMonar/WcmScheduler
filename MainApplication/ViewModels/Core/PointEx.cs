using System.Windows;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// Point構造体に対する座標演算・スナップ・クランプ処理を提供する拡張メソッド群。
    /// ノードエディタ内の座標計算を簡潔に記述するために使用する。
    /// </summary>
    public static class PointEx
    {
        /// <summary>
        /// 指定したオフセット(dx, dy)を加算した新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="dx">X方向の加算値</param>
        /// <param name="dy">Y方向の加算値</param>
        /// <returns>加算後の座標</returns>
        public static Point Add(this Point p, double dx, double dy)
            => new(p.X + dx, p.Y + dy);

        /// <summary>
        /// 指定したPointの座標値を加算した新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="other">加算する座標</param>
        /// <returns>加算後の座標</returns>
        public static Point Add(this Point p, Point other)
            => new(p.X + other.X, p.Y + other.Y);

        /// <summary>
        /// 指定したオフセット(dx, dy)を減算した新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="dx">X方向の減算値</param>
        /// <param name="dy">Y方向の減算値</param>
        /// <returns>減算後の座標</returns>
        public static Point Sub(this Point p, double dx, double dy)
            => new(p.X - dx, p.Y - dy);

        /// <summary>
        /// 指定したPointの座標値を減算した新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="other">減算する座標</param>
        /// <returns>減算後の座標</returns>
        public static Point Sub(this Point p, Point other)
            => new(p.X - other.X, p.Y - other.Y);

        /// <summary>
        /// 指定した倍率で座標を乗算した新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="factor">乗算する倍率</param>
        /// <returns>乗算後の座標</returns>
        public static Point Mul(this Point p, double factor)
            => new(p.X * factor, p.Y * factor);

        /// <summary>
        /// 指定した倍率で座標を除算した新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="factor">除算する倍率</param>
        /// <returns>除算後の座標</returns>
        public static Point Div(this Point p, double factor)
            => new(p.X / factor, p.Y / factor);

        /// <summary>
        /// 原点に対して点対称(180度回転)した座標を返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        public static Point MirrorPoint(this Point p)
            => new(-p.X, -p.Y);

        /// <summary>
        /// 指定した中心点に対して点対称(180度回転)した座標を返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="center">対象の中心点</param>
        public static Point MirrorPoint(this Point p, Point center)
            => new(
                2 * center.X - p.X,
                2 * center.Y - p.Y
            );

        /// <summary>
        /// 原点のX軸に対して線対称にした座標を返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        public static Point MirrorX(this Point p)
            => new(p.X, -p.Y);

        /// <summary>
        /// 指定した中心点のX軸に対して線対称にした座標を返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="center">対象の中心点</param>
        public static Point MirrorX(this Point p, Point center)
            => new(p.X, 2 * center.Y - p.Y);

        /// <summary>
        /// 原点のY軸に対して線対称にした座標を返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        public static Point MirrorY(this Point p)
            => new(-p.X, p.Y);

        /// <summary>
        /// 指定した中心点のY軸に対して線対称にした座標を返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="center">対象の中心点</param>
        public static Point MirrorY(this Point p, Point center)
            => new(2 * center.X - p.X, p.Y);

        /// <summary>
        /// 指定したグリッドサイズに基づき、四捨五入してスナップした座標を返す。
        /// ノード位置のスナップなどに使用する。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="grid">スナップ単位となるグリッドサイズ</param>
        /// <returns>スナップ後の座標</returns>
        public static Point RoundSnap(this Point p, double grid)
            => new(
                Math.Round(p.X / grid) * grid,
                Math.Round(p.Y / grid) * grid
            );

        /// <summary>
        /// 指定したグリッドサイズに基づき、切り捨ててスナップした座標を返す。
        /// グリッド線の原点計算などに使用する。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="grid">スナップ単位となるグリッドサイズ</param>
        /// <returns>切り捨てスナップ後の座標</returns>
        public static Point FloorSnap(this Point p, double grid)
            => new(
                Math.Floor(p.X / grid) * grid,
                Math.Floor(p.Y / grid) * grid
            );

        /// <summary>
        /// 指定した最小値・最大値の範囲内に座標を収めた新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="min">許容される最小座標</param>
        /// <param name="max">許容される最大座標</param>
        /// <returns>クランプ後の座標</returns>
        public static Point Clamp(this Point p, Point min, Point max)
            => new(
                Math.Max(min.X, Math.Min(p.X, max.X)),
                Math.Max(min.Y, Math.Min(p.Y, max.Y))
            );

        /// <summary>
        /// 指定した矩形(Rect)の範囲内に座標を収めた新しいPointを返す。
        /// </summary>
        /// <param name="p">基準となる座標</param>
        /// <param name="rect">許容範囲を表す矩形</param>
        /// <returns>クランプ後の座標</returns>
        public static Point Clamp(this Point p, Rect rect)
            => new(
                Math.Max(rect.X, Math.Min(p.X, rect.X + rect.Width)),
                Math.Max(rect.Y, Math.Min(p.Y, rect.Y + rect.Height))
            );
    }
}

/* --- End of file --- */
