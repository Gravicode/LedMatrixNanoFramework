// Decompiled with JetBrains decompiler
// Type: GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812.WS2812Controller
// Assembly: GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C134430A-513E-48D5-BBF8-C38E1D8B6560
// Assembly location: D:\experiment\LedMatrix\src\LedMatrix\bin\Debug\GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812.dll

using GHIElectronics.TinyCLR.Devices.Gpio;
using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Drivers.Worldsemi.WS2812
{
  public class WS2812Controller
  {
    private readonly GpioPin gpioPin;
    private readonly uint numLeds;
    private readonly byte[] data;
    private WS2812Controller.DataFormat bpp;
    private long resetPulse = 1000;

    public byte[] Buffer => this.data;

    public TimeSpan ResetPulse
    {
      get => TimeSpan.FromTicks(this.resetPulse);
      set => this.resetPulse = value.Ticks;
    }

    public WS2812Controller(GpioPin dataPin, uint numLeds, WS2812Controller.DataFormat bpp)
    {
      this.gpioPin = dataPin;
      this.numLeds = numLeds;
      this.bpp = bpp;
      this.data = bpp != WS2812Controller.DataFormat.rgb888 ? new byte[(int) this.numLeds * 2] : new byte[(int) this.numLeds * 3];
      this.gpioPin.SetDriveMode(GpioPinDriveMode.Output);
    }

    public void SetColor(int index, byte red, byte green, byte blue)
    {
      if (this.bpp == WS2812Controller.DataFormat.rgb888)
      {
        this.data[index * 3] = green;
        this.data[index * 3 + 1] = red;
        this.data[index * 3 + 2] = blue;
      }
      else
      {
        red >>= 3;
        green >>= 2;
        blue >>= 3;
        ushort num = (ushort) ((uint) ((int) red << 11 | (int) green << 5) | (uint) blue);
        this.data[index * 2 + 1] = (byte) ((uint) num >> 8);
        this.data[index * 2] = (byte) num;
      }
    }

    public void Flush() => this.Flush(true);

    public void Flush(bool reset)
    {
      this.NativeFlush(this.gpioPin.PinNumber, this.data, 0, this.data.Length, this.bpp == WS2812Controller.DataFormat.rgb888);
      if (!reset)
        return;
      this.Reset();
    }

    public void Clear()
    {
      for (int index = 0; (long) index < (long) this.numLeds; ++index)
        this.SetColor(index, (byte) 0, (byte) 0, (byte) 0);
    }

    public void Reset()
    {
      this.gpioPin.Write(GpioPinValue.Low);
      long num = DateTime.Now.Ticks + this.resetPulse;
      do
        ;
      while (DateTime.Now.Ticks < num);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    private extern void NativeFlush(
      int dataPin,
      byte[] buffer8,
      int offset,
      int size,
      bool bpp24);

    public enum DataFormat
    {
      rgb888,
      rgb565,
    }
  }
}
