﻿using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AntDesign.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AntDesign.Internal
{
    public partial class OverlayTrigger : AntDomComponentBase
    {
        [CascadingParameter(Name = "PrefixCls")]
        public string PrefixCls { get; set; } = "ant-dropdown";

        private string _popupContainerSelectorFromCascade = "";

        [CascadingParameter(Name = "PopupContainerSelector")]
        public string PopupContainerSelectorFromCascade
        {
            get
            {
                return _popupContainerSelectorFromCascade;
            }
            set
            {
                _popupContainerSelectorFromCascade = value;
                PopupContainerSelector = value;
            }
        }

        [Parameter]
        public string PopupContainerSelector { get; set; } = "body";

        [Parameter]
        public string PlacementCls { get; set; }

        [Parameter]
        public string OverlayEnterCls { get; set; }

        [Parameter]
        public string OverlayLeaveCls { get; set; }

        [Parameter]
        public string OverlayHiddenCls { get; set; }

        [Parameter]
        public string OverlayClassName { get; set; }

        [Parameter]
        public string OverlayStyle { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public bool Visible { get; set; } = false;
        /// <summary>
        /// 自动关闭功能和Visible参数同时生效
        /// Both auto-off and Visible control close
        /// </summary>
        [Parameter]
        public bool ComplexAutoCloseAndVisible { get; set; } = false;

        [Parameter]
        public bool IsButton { get; set; } = false;

        [Parameter]
        public bool InlineFlexMode { get; set; } = false;

        [Parameter]
        public bool HiddenMode { get; set; } = false;

        [Parameter]
        public TriggerType[] Trigger { get; set; } = new TriggerType[] { TriggerType.Hover };

        [Parameter]
        public PlacementType Placement { get; set; } = PlacementType.BottomLeft;

        [Parameter] public Action OnMouseEnter { get; set; }

        [Parameter] public Action OnMouseLeave { get; set; }

        [Parameter]
        public EventCallback<bool> OnVisibleChange { get; set; }

        [Parameter]
        public EventCallback<bool> OnOverlayHiding { get; set; }

        [Parameter]
        public RenderFragment Overlay { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }

        [Inject]
        private DomEventService DomEventService { get; set; }

        private bool _mouseInTrigger = false;
        private bool _mouseInOverlay = false;

        protected Overlay _overlay = null;

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                DomEventService.AddEventListener("document", "mouseup", OnMouseUp, false);
            }

            base.OnAfterRender(firstRender);
        }

        protected override void Dispose(bool disposing)
        {
            DomEventService.RemoveEventListerner<JsonElement>("document", "mouseup", OnMouseUp);
            base.Dispose(disposing);
        }

        protected virtual async Task OnTriggerMouseEnter()
        {
            _mouseInTrigger = true;

            if (_overlay != null && IsContainTrigger(TriggerType.Hover))
            {
                _overlay.PreventHide(true);

                await Show();
            }

            OnMouseEnter?.Invoke();
        }

        protected virtual async Task OnTriggerMouseLeave()
        {
            _mouseInTrigger = false;

            if (_overlay != null && IsContainTrigger(TriggerType.Hover))
            {
                _overlay.PreventHide(_mouseInOverlay);

                await Hide();
            }

            OnMouseLeave?.Invoke();
        }

        protected virtual async Task OnTriggerFocusIn()
        {
            _mouseInTrigger = true;

            if (_overlay != null && IsContainTrigger(TriggerType.Focus))
            {
                _overlay.PreventHide(true);

                await Show();
            }
        }

        protected virtual async Task OnTriggerFocusOut()
        {
            _mouseInTrigger = false;

            if (_overlay != null && IsContainTrigger(TriggerType.Focus))
            {
                _overlay.PreventHide(_mouseInOverlay);

                await Hide();
            }
        }

        protected virtual void OnOverlayMouseEnter()
        {
            _mouseInOverlay = true;

            if (_overlay != null && IsContainTrigger(TriggerType.Hover))
            {
                _overlay.PreventHide(true);
            }
        }

        protected virtual async Task OnOverlayMouseLeave()
        {
            _mouseInOverlay = false;

            if (_overlay != null && IsContainTrigger(TriggerType.Hover))
            {
                _overlay.PreventHide(_mouseInTrigger);

                await Hide();
            }
        }

        protected virtual async Task OnClickDiv(MouseEventArgs args)
        {
            if (!IsButton)
            {
                await OnTriggerClick();
            }
            else
            {
                await OnClick.InvokeAsync(args);
            }
        }

        protected virtual async Task OnTriggerClick()
        {
            if (IsContainTrigger(TriggerType.Click))
            {
                if (_overlay.IsPopup())
                {
                    await Hide();
                }
                else
                {
                    await Show();
                }
            }
            else if (IsContainTrigger(TriggerType.ContextMenu) && _overlay.IsPopup())
            {
                await Hide();
            }
        }

        protected virtual async Task OnTriggerContextmenu(MouseEventArgs args)
        {
            if (IsContainTrigger(TriggerType.ContextMenu))
            {
                int offsetX = 10;
                int offsetY = 10;
#if NET5_0
                // offsetX/offsetY were only supported in Net5
                offsetX = (int)args.OffsetX;
                offsetY = (int)args.OffsetY;
#endif

                await Hide();
                await Show(offsetX, offsetY);
            }
        }

        protected virtual void OnMouseUp(JsonElement element)
        {
            if (_mouseInOverlay == false && _mouseInTrigger == false)
            {
                Hide();
            }
        }

        protected virtual bool IsContainTrigger(TriggerType triggerType)
        {
            return Trigger.Contains(triggerType);
        }

        protected virtual async Task OverlayVisibleChange(bool visible)
        {
            await OnVisibleChange.InvokeAsync(visible);
        }

        protected virtual async Task OverlayHiding(bool visible)
        {
            await OnOverlayHiding.InvokeAsync(visible);
        }

        protected virtual void OnOverlayShow() { }
        protected virtual void OnOverlayHide() { }

        internal virtual string GetPlacementClass()
        {
            if (!string.IsNullOrEmpty(PlacementCls))
            {
                return PlacementCls;
            }
            return $"{PrefixCls}-placement-{Placement.Name}";
        }

        internal virtual string GetOverlayEnterClass()
        {
            if (!string.IsNullOrEmpty(OverlayEnterCls))
            {
                return OverlayEnterCls;
            }
            return $"slide-{Placement.SlideName}-enter slide-{Placement.SlideName}-enter-active slide-{Placement.SlideName}";
        }

        internal virtual string GetOverlayLeaveClass()
        {
            if (!string.IsNullOrEmpty(OverlayLeaveCls))
            {
                return OverlayLeaveCls;
            }
            return $"slide-{Placement.SlideName}-leave slide-{Placement.SlideName}-leave-active slide-{Placement.SlideName}";
        }

        internal virtual string GetOverlayHiddenClass()
        {
            if (!string.IsNullOrEmpty(OverlayHiddenCls))
            {
                return OverlayHiddenCls;
            }
            return $"{PrefixCls}-hidden";
        }

        internal virtual async Task Show(int? overlayLeft = null, int? overlayTop = null)
        {
            await _overlay.Show(overlayLeft, overlayTop);
        }

        internal virtual async Task Hide(bool force = false)
        {
            if (Visible && !ComplexAutoCloseAndVisible && !force)
            {
                return;
            }

            await _overlay.Hide(force);
        }

        internal Overlay GetOverlayComponent()
        {
            return _overlay;
        }

        internal async Task<Element> GetTriggerDomInfo()
        {
            return await JsInvokeAsync<Element>(JSInteropConstants.GetFirstChildDomInfo, Ref);
        }

        public async Task Close()
        {
            await _overlay.Hide(true);
        }

        public bool IsOverlayShow()
        {
            return _overlay != null ? _overlay.IsPopup() : false;
        }
    }
}
