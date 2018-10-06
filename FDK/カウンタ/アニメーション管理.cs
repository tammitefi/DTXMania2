using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Animation;

namespace FDK.カウンタ
{
    public class アニメーション管理 : IDisposable
    {
        public Manager Manager
            => this._Manager;
        public Timer Timer
            => this._Timer;
        public TransitionLibrary TrasitionLibrary
            => this._TrasitionLibrary;

        public アニメーション管理()
        {
            this._Manager = new Manager();
            this._Timer = new Timer();
            this._TrasitionLibrary = new TransitionLibrary();
        }
        public void 進行する()
        {
            this._Manager.Update( this._Timer.Time );
        }
        public void Dispose()
        {
            this._TrasitionLibrary?.Dispose();
            this._TrasitionLibrary = null;

            this._Timer?.Dispose();
            this._Timer = null;

            this._Manager?.Dispose();
            this._Manager = null;
        }

        private Manager _Manager = null;
        private Timer _Timer = null;
        private TransitionLibrary _TrasitionLibrary = null;
    }
}