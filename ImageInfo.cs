// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace animatorapp
{
    /// <summary>
    /// Animates one or more images that have time-based frames. This file contains the nested ImageInfo class
    /// - See ImageAnimator.cs for the definition of the outer class.
    /// </summary>
    public sealed partial class ImageAnimator
    {
        /// <summary>
        /// ImageAnimator nested helper class used to store extra image state info.
        /// </summary>
        internal sealed class ImageInfo
        {
            private const int PropertyTagFrameDelay = 0x5100;

            internal readonly Image _image;
            private int _frame;
            private readonly int _frameCount;
            private bool _frameDirty;
            private readonly bool _animated;
            private EventHandler? _onFrameChangedHandler;
            private readonly int[] _frameEndTimes;
            private int _frameTimer;

            public ImageInfo(Image image)
            {
                _image = image;
                _animated = ImageAnimator.CanAnimate(image);
                _frameEndTimes = null!; // guaranteed to be initialized by the final check

                if (_animated)
                {
                    _frameCount = image.GetFrameCount(FrameDimension.Time);

                    PropertyItem? frameDelayItem = image.GetPropertyItem(PropertyTagFrameDelay);

                    // If the image does not have a frame delay, we just return 0.
                    if (frameDelayItem != null)
                    {
                        // Convert the frame delay from byte[] to int
                        byte[] values = frameDelayItem.Value!;

                        // On Windows, the frame delay bytes are repeated such that the array
                        // length is 4 times the framecount. On Linux, the frame delay bytes
                        // are not repeated if the same delay applies to all frames.
                        Debug.Assert(FrameCount % (values.Length / 4) == 0, "PropertyItem has invalid value byte array. The FrameCount should be evenly divisible by a quarter of the byte array's length.");

                        _frameEndTimes = new int[FrameCount];

                        for (int f = 0, i = 0; f < FrameCount; ++f, i += 4)
                        {
                            if (i == values.Length)
                            {
                                i = 0;
                            }

                            // Frame delays are stored in 1/100ths of a second; convert to milliseconds while accumulating
                            _frameEndTimes[f] = (f > 0 ? _frameEndTimes[f - 1] : 0) + (BitConverter.ToInt32(values, i) * 10);
                        }
                    }
                }
                else
                {
                    _frameCount = 1;
                }
                if (_frameEndTimes == null)
                {
                    _frameEndTimes = new int[FrameCount];
                }
            }

            /// <summary>
            /// Whether the image supports animation.
            /// </summary>
            public bool Animated
            {
                get
                {
                    return _animated;
                }
            }

            /// <summary>
            /// The current frame.
            /// </summary>
            public int Frame
            {
                get
                {
                    return _frame;
                }
            }

            public int FrameEndTime
            {
                get
                {
                    return _frameEndTimes[_frame];
                }
            }

            /// <summary>
            /// The current frame has not been updated.
            /// </summary>
            public bool FrameDirty
            {
                get
                {
                    return _frameDirty;
                }
            }

            public EventHandler? FrameChangedHandler
            {
                get
                {
                    return _onFrameChangedHandler;
                }
                set
                {
                    _onFrameChangedHandler = value;
                }
            }

            /// <summary>
            /// The number of frames in the image.
            /// </summary>
            public int FrameCount
            {
                get
                {
                    return _frameCount;
                }
            }

            /// <summary>
            /// The total animation time of the image, in milliseconds
            /// </summary>
            public int TotalAnimationTime
            {
                get
                {
                    if (Animated)
                    {
                        return _frameEndTimes[_frameCount - 1];
                    }

                    return 0;
                }
            }

            /// <summary>
            /// The current time into the animation, in milliseconds
            /// </summary>
            public int AnimationTimer
            {
                get
                {
                    return _frameTimer;
                }
            }

            public void AdvanceAnimationBy(int milliseconds)
            {
                int oldFrame = _frame;
                _frameTimer += milliseconds;

                if (_frameTimer > TotalAnimationTime)
                {
                    _frameTimer %= TotalAnimationTime;
                }

                if (_frame > 0 && _frameTimer < _frameEndTimes[_frame - 1])
                {
                    _frame = 0;
                }

                while (_frameTimer > _frameEndTimes[_frame])
                {
                    _frame++;
                }

                if (_frame != oldFrame)
                {
                    _frameDirty = true;
                    OnFrameChanged();
                }
            }            

            /// <summary>
            /// The image this object wraps.
            /// </summary>
            public Image Image
            {
                get
                {
                    return _image;
                }
            }

            /// <summary>
            /// Selects the current frame as the active frame in the image.
            /// </summary>
            public void UpdateFrame()
            {
                if (_frameDirty)
                {
                    _image.SelectActiveFrame(FrameDimension.Time, Frame);
                    _frameDirty = false;
                }
            }

            /// <summary>
            /// Raises the FrameChanged event.
            /// </summary>
            private void OnFrameChanged()
            {
                if (_onFrameChangedHandler != null)
                {
                    _onFrameChangedHandler(this, EventArgs.Empty);
                }
            }
        }
    }
}
