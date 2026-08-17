import { Browser, EventHandler } from '@syncfusion/ej2-base';
import { addClass } from '@syncfusion/ej2-base';
import { formatUnit, isNullOrUndefined } from '@syncfusion/ej2-base';
import { getScrollBarWidth, getUpdateUsingRaf, getSiblingsHeight } from './util';
import { SfGrid } from './sf-grid-fn';
import { calculateRelativeBasedPosition } from '@syncfusion/ej2-popups';
import { Column} from './interfaces';
import { parentsUntil} from './util';

/**
 * The `Scroll` module is used to handle scrolling behaviour.
 */
export class Scroll {
    private parent: SfGrid;
    //To maintain scroll state on grid actions.
    public previousValues: { top: number, left: number } = { top: 0, left: 0 };
    private oneTimeReady: boolean = true;
    public content: HTMLDivElement;
    private header: HTMLDivElement;
    private pageXY: { x: number, y: number };
    private gridParentElement: HTMLElement;

    /**
     * Constructor for the Grid scrolling.
     *
     * @param {SfGrid} [parent] - Optional. The parent grid instance.
     * @hidden
     */
    constructor(parent?: SfGrid) {
        this.parent = parent;
        this.addEventListener();
        this.setHeight();
        this.setPadding();
    }

    /**
     * Sets the height for something (describe what is being set the height for).
     *
     * @returns {void}
     * @hidden
     */
    public setHeight(): void {
        let mHdrHeight: number = 0;
        const content: HTMLElement = (<HTMLElement>this.parent.element.querySelector('.e-content'));
        if (this.parent.options.frozenRows && this.parent.options.height !== 'auto' && !this.parent.options.height.match(/%/g)) {
            const tbody: HTMLElement = (this.parent.element.querySelector('.e-headercontent').querySelector('tbody') as HTMLElement);
            mHdrHeight = tbody ? tbody.offsetHeight : 0;
            content.style.height = formatUnit((parseInt(this.parent.options.height, 10) - mHdrHeight));
        }
    }
    /**
     * @hidden
     */

    public removeUnwantedScroll(offsetValue: string, minWidth: number = 0): boolean {
        let isFrozenColumn: boolean = false;
        const isFrozenRowWidth: boolean = this.parent.content.offsetWidth >= (this.parent.getContentTable() as HTMLElement).offsetWidth;
        const isFrozenRow: boolean = this.parent.options.frozenRows !== 0 && this.parent.options.frozenColumns === 0;
        //const movablescrollbarDiv: HTMLElement = this.parent.element.querySelector('.e-movablescrollbar');
        //let movableScrollbarHeight: number = movablescrollbarDiv ? movablescrollbarDiv.offsetHeight : 0;
        if (this.parent.options.frozenColumns !== 0 && !isNullOrUndefined(this.getMovableContent())) {
            isFrozenColumn = this.getMovableContent().offsetWidth >= ((this.getMovableContentTable().offsetWidth + minWidth) - 2) ||
            this.getMovableContent().offsetWidth === 0;
        }
        const isHorizontalScrollBarRendered: boolean = this.parent.content.scrollWidth > this.parent.content.offsetWidth;
        const tableHeight: number = (this.parent.getContentTable() as HTMLElement).offsetHeight;
        const actualScrollHeight: number = tableHeight + (isHorizontalScrollBarRendered ? Scroll.getScrollBarWidth() : 0);
        if (offsetValue === 'Height' && this.parent.content.offsetHeight >= actualScrollHeight) {
            return true;
        }
        if (offsetValue === 'Width' && (isFrozenColumn || (isFrozenRow && isFrozenRowWidth))) {
            if (this.parent.options.height === '100%') {
                this.frozenContentElement().style.borderBottom = 'none';
            }
            return true;
        }
        return false;
    }
    /**
     * @hidden
     */

    public getMovableContent(): HTMLElement {
        return this.parent.element.querySelector('.e-movablecontent');
    }
    /**
     * @hidden
     */

    public getMovableContentTable(): HTMLElement {
        return this.parent.element.querySelector('.e-movablecontent .e-table');
    }
    /**
     * @hidden
     */

    public frozenContentElement(): HTMLElement {
        return this.parent.element.querySelector('.e-frozencontent');
    }
    /**
     * Retrieves the scrollbar element within the parent element.
     *
     * @returns {HTMLElement} The scrollbar element.
     * @hidden
     */
    public getScrollbar(): HTMLElement {
        return this.parent.element.querySelector('.e-scrollbar');
    }
    /**
     * Sets padding for the content based on certain conditions.
     *
     * @returns {void}
     * @hidden
     */

    public setPadding(): void {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const content: any = <HTMLElement>this.parent.element.querySelector('.e-gridheader');
        if (isNullOrUndefined(content)) { return; }
        if (this.parent.options.height === 'auto' && this.parent.options.frozenName === 'None' &&
            this.parent.options.frozenColumns === 0) { return; }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const footer: any = this.parent.element.querySelector('.e-gridfooter');
        if (this.removeUnwantedScroll('Height')) {
            this.content.style.overflow = this.parent.options.frozenColumns && this.parent.options.enableColumnVirtualization ? 'hidden auto' : 'auto';
            (<HTMLElement>content.querySelector('.e-headercontent')).style.borderRightWidth = '';
            content.style.paddingRight = '';
            if (!isNullOrUndefined(footer) && footer) {
                footer.style.paddingRight = '';
            }
            return;
        }
        let scrollWidth: number = Scroll.getScrollBarWidth() - this.getThreshold();
        const cssProps: ScrollCss = this.getCssProperties();
        if (this.parent.options.enableRtl) {
            content.style['padding-right'] = '';
        }
        else {
            content.style['padding-left'] = '';
        }
        const contentElement: HTMLElement = this.parent.element.querySelector('.e-content') as HTMLElement;
        const overflowy: string = window.getComputedStyle(contentElement)['overflow-y'];
        const overflowHeight: number = contentElement.clientHeight - this.parent.getContentTable().clientHeight;
        if (!this.parent.options.enableVirtualization && this.parent.options.frozenColumns > 0 && (this.parent.options.height === 'auto' || overflowHeight >= 0)
            && overflowy === 'auto' && (this.parent.options.allowGrouping || !this.parent.options.hasDetailTemplate)) {
            scrollWidth = 0;
        }
        content.style[cssProps.padding] = scrollWidth > 0 ? scrollWidth + 'px' : '0px';
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (<any>content.querySelector('.e-headercontent')).style[cssProps.border] = scrollWidth > 0 ? '1px' : '0px';
        if (footer) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const footerContent: any = footer.querySelector('.e-summarycontent');
            if (!this.parent.options.enableAdaptiveUI) {
                footerContent.style[cssProps.border] = scrollWidth > 0 ? '1px' : '0px';
            }
            footer.style[cssProps.padding] = scrollWidth > 0 ? scrollWidth + 'px' : '0px';
        }
    }
    /**
     * Removes padding from header and its parent element based on the right-to-left (RTL) mode.
     *
     * @param {boolean} [rtl] - Optional. Specifies whether to enable RTL mode.
     * @returns {void}
     * @hidden
     */
    public removePadding(rtl?: boolean): void {
        const cssProps: ScrollCss = this.getCssProperties(rtl);
        const hDiv: HTMLDivElement = (<HTMLDivElement>this.parent.getHeaderContent());
        hDiv.style[cssProps.border] = '';
        hDiv.parentElement.style[cssProps.padding] = '';
        //const footerDiv: HTMLDivElement = (<HTMLDivElement>this.parent.getFooterContent());
    }
    /**
     * Refresh makes the Grid adoptable with the height of parent container.
     *
     * > The [`height`](grid/#height/) must be set to 100%.
     *
     * @returns {void}
     */
    public refresh(): void {
        if (this.parent.options.height !== '100%') {
            return;
        }

        const content: HTMLElement = <HTMLElement>this.parent.element.querySelector('.e-gridcontent');
        const height: number = getSiblingsHeight(content);
        content.style.height = 'calc(100% - ' + height + 'px)'; //Set the height to the '.e-gridcontent';
    }

    public getThreshold(): number {
        /* Some browsers places the scroller outside the content,
         * hence the padding should be adjusted.*/
        const appName: string = Browser.info.name;
        const userAgent: string = navigator.userAgent;
        if (userAgent.indexOf('Edg/') > -1) {
            return 0.75;
        }
        if (appName === 'mozilla') {
            return 0.5;
        }
        return 1;
    }
    /**
     *
     * @returns {void}
     * @hidden
     */
    public addEventListener(): void {
        this.wireEvents();
        // this.parent.on(onEmpty, this.wireEvents, this);
        // this.parent.on(contentReady, this.wireEvents, this);
        // this.parent.on(uiUpdate, this.onPropertyChanged, this);
        // this.parent.on(textWrapRefresh, this.wireEvents, this);
        // this.parent.on(headerRefreshed, this.setScrollLeft, this);
    }

    // private setScrollLeft(): void {
    //     if (this.parent.options.frozenColumns) {
    //         (<HTMLElement>(<SfGrid>this.parent).headerModule.getMovableHeader()).scrollLeft = this.previousValues.left;
    //     }
    // }

    private onContentScroll(scrollTarget: HTMLElement): Function {
        const element: HTMLElement = scrollTarget;
        const isHeader: boolean = element.classList.contains('e-headercontent');
        return (e: Event) => {
            if (this.content.querySelector('tbody') === null || this.parent.options.isPreventScrollEvent) {
                return;
            }

            const target: HTMLElement = (<HTMLElement>e.target);
            const left: number = target.scrollLeft;
            const gridElement: Element = parentsUntil(target, 'e-grid');
            if (!isNullOrUndefined(gridElement) && gridElement.querySelectorAll('.e-filter-popup.e-popup-open').length > 0 && this.previousValues.left !== left) {
                this.parent.dotNetRef.invokeMethodAsync('FilterPopupClose');
            }
            // Close enhanced operator dropdowns on scroll
            // Enhanced operator dropdowns are rendered outside the grid element in the DOM
            if (!isNullOrUndefined(gridElement) && this.previousValues.left !== left) {
                this.parent.closeOperatorDropdownIfOpen();
            }
            //const sLimit: number = target.scrollWidth;
            this.updateFrozenShadow(target);
            const frozenRightColumns: Column[] = this.parent.getColumns().filter((a: Column) => {
                return a.isFrozen && a.freeze === 'Right';
            });
            if (this.content.scrollTop > 0 && this.parent.options.frozenRows) {
                this.parent.element.classList.add('e-top-shadow');
            } else {
                this.parent.element.classList.remove('e-top-shadow');
            }
            const widthVal: number = Math.round((target.scrollWidth - target.scrollLeft));
            const gridcontent: HTMLElement = this.parent.getContent();
            //The small margin (e.g., 1) accounts for floating-point precision.
            const scrollReachRightEnd: boolean = gridcontent.scrollLeft >= gridcontent.scrollWidth - gridcontent.clientWidth - 1;
            const parentElement: HTMLElement = this.parent.element;
            if (frozenRightColumns.length > 0 && (widthVal === target.offsetWidth || scrollReachRightEnd)) {
                parentElement.classList.remove('e-right-shadow');
            } else {
                parentElement.classList.add('e-right-shadow');
            }
            const isFooter: boolean = target.classList.contains('e-summarycontent');

            if (this.parent.options.enableInfiniteScrolling && !this.parent.options.isEdit) {
                if (!this.parent.infiniteScrollModule.isLazyChildLoad) {
                    this.parent.infiniteScrollModule.infiniteScrollHandler(target, this.previousValues.left, false);
                }
            }
            if (this.parent.options.groupCount > 0 && this.parent.options.enableLazyLoading) {
                const isDown: boolean = this.previousValues.top < target.scrollTop;
                if (isDown) {
                    this.parent.infiniteScrollModule.lazyLoadInfiniteScrollHandler(isDown);
                }
            }
            if (this.previousValues.left === left) {
                this.previousValues.top = !isHeader ? this.previousValues.top : target.scrollTop;
                return;
            }

            element.scrollLeft = left;
            const footer: HTMLElement = this.parent.element.querySelector('.e-summarycontent');
            if (footer) {
                footer.scrollLeft = left;
            }
            if (isFooter) { this.header.scrollLeft = left; }
            this.previousValues.left = left;
        };
    }

    private onFreezeContentScroll(scrollTarget: HTMLElement): Function {
        const element: HTMLElement = scrollTarget;
        return (e: Event) => {
            if (this.content.querySelector('tbody') === null) {
                return;
            }
            const target: HTMLElement = <HTMLElement>e.target;
            const top: number = target.scrollTop;
            if (this.previousValues.top === top) {
                return;
            }
            element.scrollTop = top;
            this.previousValues.top = top;
        };
    }
    private updateFrozenShadow(target: HTMLElement): void {
        const frozenLeftColumns: Column[] = this.parent.getColumns().filter((a: Column) => {
            return a.isFrozen && a.freeze === 'Left';
        });
        const frozenRightColumns: Column[] = this.parent.getColumns().filter((a: Column) => {
            return a.isFrozen && a.freeze === 'Right';
        });
        if (target.scrollLeft !== 0 && ((this.parent.options.frozenColumns > 0 && frozenRightColumns.length === 0) ||
        frozenLeftColumns.length > 0)) {
            this.parent.element.classList.add('e-left-shadow');
        } else if (this.parent.element.classList.contains('e-left-shadow')) {
            this.parent.element.classList.remove('e-left-shadow');
        }
    }
    private onCustomScrollbar(mCont: HTMLElement, mHdr: HTMLElement): Function {
        const content: HTMLElement = mCont;
        const header: HTMLElement = mHdr;
        let mfooter: HTMLElement;
        return (e: Event) => {
            if (this.content.querySelector('tbody') === null) {
                return;
            }

            const target: HTMLElement = <HTMLElement>e.target;
            const left: number = target.scrollLeft;
            if (this.previousValues.left === left) {
                return;
            }
            this.updateFrozenShadow(target);
            if (this.parent.options.aggregatesCount) {
                mfooter = this.parent.element.querySelector('.e-movablefootercontent');
            }
            content.scrollLeft = left;
            header.scrollLeft = left;
            if (mfooter) {
                mfooter.scrollLeft = left;
            }
            this.previousValues.left = left;

        };
    }
    private onWheelScroll(scrollTarget: HTMLElement): Function {
        const element: HTMLElement = scrollTarget;
        return (e: WheelEvent) => {
            if (this.content.querySelector('tbody') === null) {
                return;
            }
            const top: number = element.scrollTop + (e.deltaMode === 1 ? e.deltaY * 30 : e.deltaY);
            if (this.previousValues.top === top) {
                return;
            }
            e.preventDefault();
            this.parent.getContent().querySelector('.e-frozencontent').scrollTop = top;
            element.scrollTop = top;
            this.previousValues.top = top;
        };
    }

    private onTouchScroll(scrollTarget: HTMLElement): Function {
        const element: HTMLElement = scrollTarget;
        return (e: PointerEvent | TouchEvent) => {
            if ((e as PointerEvent).pointerType === 'mouse') {
                return;
            }
            const isFrozen: boolean = this.parent.options.frozenColumns > 0;
            const pageXY: { x: number, y: number } = this.getPointXY(e);
            const left: number = element.scrollLeft + (this.pageXY.x - pageXY.x);
            const mHdr: Element = isFrozen ?
                this.parent.getHeaderContent() :
                this.parent.getHeaderContent().querySelector('.e-headercontent') as Element;
            const mCont: Element = isFrozen ?
                this.parent.getContent() :
                this.parent.getContent().querySelector('.e-content') as Element;
            if (this.previousValues.left === left || (left < 0 || (mHdr.scrollWidth - mHdr.clientWidth) < left)) {
                return;
            }
            if ((e as Event).cancelable) {
                e.preventDefault();
            }
            mHdr.scrollLeft = left;
            mCont.scrollLeft = left;
            if (isFrozen) {
                const scrollBar: HTMLElement = this.parent.element.querySelector('.e-movablescrollbar');
                scrollBar.scrollLeft = left;
            }
            this.pageXY.x = pageXY.x;
            this.previousValues.left = left;
            // let cont: Element;
            // let mHdr: Element;
            // let pageXY: { x: number, y: number } = this.getPointXY(e);
            // let top: number = element.scrollTop + (this.pageXY.y - pageXY.y);
            // let left: number = element.scrollLeft + (this.pageXY.x - pageXY.x);
            // if (this.parent.getHeaderContent().contains(e.target as Element)) {
            //     mHdr = this.parent.options.frozenColumns ?
            //         this.parent.getHeaderContent().querySelector('.e-movableheader') :
            //         this.parent.getHeaderContent().querySelector('.e-headercontent') as Element;
            //     if (this.previousValues.left === left || (left < 0 || (mHdr.scrollWidth - mHdr.clientWidth) < left)) {
            //         return;
            //     }
            //     e.preventDefault();
            //     mHdr.scrollLeft = left;
            //     element.scrollLeft = left;
            //     this.pageXY.x = pageXY.x;
            //     this.previousValues.left = left;
            // } else {
            //     cont = this.parent.getContent().querySelector('.e-frozencontent');
            //     if (this.previousValues.top === top && (top < 0 || (cont.scrollHeight - cont.clientHeight) < top)
            //         || (top < 0 || (cont.scrollHeight - cont.clientHeight) < top)) {
            //         return;
            //     }
            //     e.preventDefault();
            //     cont.scrollTop = top;
            //     element.scrollTop = top;
            //     this.pageXY.y = pageXY.y;
            //     this.previousValues.top = top;
            // }
        };
    }

    private setPageXY(): Function {
        return (e: PointerEvent | TouchEvent) => {
            if ((e as PointerEvent).pointerType === 'mouse') {
                return;
            }
            this.pageXY = this.getPointXY(e);
        };
    }

    private getPointXY(e: PointerEvent | TouchEvent): { x: number, y: number } {
        const pageXY: { x: number, y: number } = { x: 0, y: 0 };
        if ((e as TouchEvent).touches && (e as TouchEvent).touches.length) {
            pageXY.x = (e as TouchEvent).touches[0].pageX;
            pageXY.y = (e as TouchEvent).touches[0].pageY;
        } else {
            pageXY.x = (e as PointerEvent).pageX;
            pageXY.y = (e as PointerEvent).pageY;
        }
        return pageXY;
    }

    private wireEvents(): void {
        if (this.oneTimeReady) {
            this.content = <HTMLDivElement>this.parent.getContent();
            this.header = <HTMLDivElement>this.parent.getHeaderContent();
            const mScrollBar: HTMLElement = this.content.parentElement.querySelector('.e-movablescrollbar') as HTMLElement;
            const root: HTMLElement = this.parent && this.parent.element;
            if (root && !root.classList.contains('e-device')) {
                const inferred: boolean = ('ontouchstart' in window) && window.matchMedia('(pointer: coarse)').matches && window.matchMedia('(hover: none)').matches;
                if (inferred) {
                    root.classList.add('e-device');
                }
            }
            //Need for custom scrollbar
            if (this.parent.options.frozenColumns > 0 && this.parent.options.enableColumnVirtualization) {
                EventHandler.add(mScrollBar, 'scroll', this.onCustomScrollbar(this.content, this.header), this);
                EventHandler.add(this.content, 'scroll', this.onCustomScrollbar(mScrollBar, this.header), this);
                EventHandler.add(this.header, 'scroll', this.onCustomScrollbar(mScrollBar, this.content), this);
                EventHandler.add(this.header, 'touchstart pointerdown', this.setPageXY(), this);
                EventHandler.add(this.content, 'touchstart pointerdown', this.setPageXY(), this);
                EventHandler.add(this.content, 'touchmove pointermove', this.onTouchScroll(this.header), this);
            }
            else {
                EventHandler.add(this.content, 'scroll', this.onContentScroll(this.header), this);
                EventHandler.add(this.header, 'scroll', this.onContentScroll(this.content), this);
            }
            if (this.parent.options.aggregatesCount) {
                const footer: HTMLElement = this.parent.element.querySelector('.e-summarycontent');
                if (!isNullOrUndefined(footer)) {
                    EventHandler.add(footer, 'scroll', this.onContentScroll(this.content), this);
                }
            }
            if (this.parent.options.enableStickyHeader) {
                this.addStickyListener(true);
            }
            this.refresh();
            this.oneTimeReady = false;
        }
        const table: Element = this.parent.getContent().querySelector('.e-table');
        let sLeft: number;
        let sHeight: number;
        let clientHeight: number;
        getUpdateUsingRaf(
            () => {
                sLeft = this.header.scrollLeft;
                sHeight = table.scrollHeight;
                clientHeight = this.parent.getContent().clientHeight;
            },
            () => {
                if (!this.parent.options.enableVirtualization) {
                    if (sHeight < clientHeight) {
                        addClass(table.querySelectorAll('tr:last-child td'), 'e-lastrowcell');
                    }
                    this.header.scrollLeft = this.previousValues.left;
                    this.content.scrollLeft = this.previousValues.left;
                    this.content.scrollTop = this.previousValues.top;
                }
                if (!this.parent.options.enableColumnVirtualization) {
                    this.content.scrollLeft = sLeft;
                }
                if (this.parent.options.frozenColumns && this.parent.getHeaderContent()) {
                    this.parent.getHeaderContent().scrollLeft = this.parent.getContent().scrollLeft;
                }
            }
        );
    }

    /**
     *  Retrieves CSS properties based on the right-to-left (RTL) mode.
     *
     * @param {boolean} [rtl] - Optional. Specifies whether to enable RTL mode.
     * @returns {ScrollCss} The CSS properties object based on the RTL mode.
     * @hidden
     */
    public getCssProperties(rtl?: boolean): ScrollCss {
        const css: ScrollCss = {};
        const enableRtl: boolean = isNullOrUndefined(rtl) ? this.parent.options.enableRtl : rtl;
        css.border = enableRtl ? 'borderLeftWidth' : 'borderRightWidth';
        css.padding = enableRtl ? 'paddingLeft' : 'paddingRight';
        return css;
    }
    /**
     * Returns the scrollable parent element of the grid parent node.
     *
     * @param {HTMLElement} gridParentNode - The grid parent node to find the scrollable parent from.
     * @returns {HTMLElement|null} The scrollable parent element, or null if not found.
     * @hidden
     */
    private getScrollableParent(gridParentNode: HTMLElement): HTMLElement {
        if (gridParentNode === null) {
            return null;
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const gridParent: any = isNullOrUndefined(gridParentNode.tagName) ?
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
            (gridParentNode as any).scrollingElement : gridParentNode;
        const overflowY: string = document.defaultView.getComputedStyle(gridParent, null).overflowY;
        if (gridParent.scrollHeight > gridParent.clientHeight && overflowY !== 'hidden' && overflowY !== 'visible' || gridParentNode.tagName === 'HTML' || gridParentNode.tagName === 'ARTICLE') {
            return gridParentNode;
        }
        else {
            return this.getScrollableParent(gridParentNode.parentNode as HTMLElement);
        }
    }

    public addStickyListener(isAdd: boolean): void {
        this.gridParentElement = this.getScrollableParent(this.parent.element.parentElement);
        if (isAdd) {
            if (this.gridParentElement) {
                EventHandler.add(this.gridParentElement.tagName === 'HTML' || this.gridParentElement.tagName === 'ARTICLE' ? document :
                    this.gridParentElement, 'scroll', this.makeStickyHeader, this);
            }
            else {
                EventHandler.remove(this.gridParentElement, 'scroll', this.makeStickyHeader);
            }
        }
    }

    private makeStickyHeader(): void {
        if (this.parent.options.enableStickyHeader && this.parent.element && !isNullOrUndefined(this.gridParentElement) &&
        this.parent.getContent()) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const contentRect: any = this.parent.getContent().parentElement.getBoundingClientRect();
            if (contentRect) {
                const headerElement: HTMLElement = this.parent.getHeaderContent().parentElement as HTMLElement;
                const toolbarElement: HTMLElement = this.parent.element.querySelector('.e-toolbar') as HTMLElement;
                const groupElement: HTMLElement = this.parent.element.querySelector('.e-groupdroparea') as HTMLElement;
                const ccElement: HTMLElement = this.parent.element.querySelector('.e-ccdlg') as HTMLElement;
                const ccToolbar: HTMLElement = this.parent.element.querySelector('.e-cc-toolbar') as HTMLElement;
                const height: number = headerElement.offsetHeight + (toolbarElement ? toolbarElement.offsetHeight : 0) +
                (groupElement ? groupElement.offsetHeight : 0);
                const parentTop: number = this.gridParentElement.getBoundingClientRect().top;
                const top1: number = contentRect.top - (parentTop < 0 ? 0 : parentTop);
                const left: number = contentRect.left;
                if (top1 < height && contentRect.bottom > 0) {
                    headerElement.classList.add('e-sticky');
                    let elemTop: number = 0;
                    if (groupElement) {
                        this.setSticky(groupElement, elemTop, contentRect.width, left, true);
                        elemTop += groupElement.getBoundingClientRect().height - 2;
                    }
                    if (toolbarElement) {
                        this.setSticky(toolbarElement, elemTop, contentRect.width, left, true);
                        elemTop += toolbarElement.getBoundingClientRect().height - 2;
                    }
                    this.setSticky(headerElement, elemTop, contentRect.width, left, true);
                }
                else {
                    if (headerElement.classList.contains('e-sticky')) {
                        this.setSticky(headerElement, null, null, null, false);
                    }
                    if (groupElement) {
                        this.setSticky(groupElement, null, null, null, false);
                    }
                    if (toolbarElement) {
                        this.setSticky(toolbarElement, null, null, null, false);
                    }
                    if (ccElement && !isNullOrUndefined(ccToolbar)) {
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        const position: any = calculateRelativeBasedPosition(ccToolbar, ccElement);
                        const ccTop: number = position.top + ccToolbar.getBoundingClientRect().height;
                        const elementVisible: string = ccElement.style.display;
                        ccElement.style.display = 'block';
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        const ccRect: any = ccElement.getBoundingClientRect();
                        const left: number = ccToolbar.getBoundingClientRect().left - contentRect.left -
                        ccRect.width + ccToolbar.clientWidth + 2;
                        ccElement.style.display = elementVisible;
                        this.setSticky(ccElement, ccTop, ccRect.width, left, false);
                    }
                }
            }
        }
    }

    private setSticky(element: HTMLElement, top?: number, width?: number, left?: number, isAdd?: boolean): void {
        if (isAdd) {
            element.classList.add('e-sticky');
        }
        else {
            element.classList.remove('e-sticky');
        }
        element.style.width = width != null ? width + 'px' : '';
        element.style.top = top != null ? top - 2 + 'px' : '';
        element.style.left = left !== null ? parseInt(element.style.left, 10) !== left ? left + 'px' : element.style.left : '';
    }

    public destroy(): void {
        const gridElement: Element = this.parent.element;
        if (!gridElement || (!gridElement.querySelector('.e-gridheader') && !gridElement.querySelector('.e-gridcontent'))) { return; }

        if (this.parent.options.enableStickyHeader) {
            this.addStickyListener(false);
        }
        //Remove padding
        this.removePadding();

        //Remove Dom event
        EventHandler.remove(<HTMLDivElement>this.parent.getContent(), 'scroll', this.onContentScroll);
        EventHandler.remove(this.header, 'scroll', this.onContentScroll);
        if (this.parent.options.aggregatesCount) {
            const footer: HTMLElement = this.parent.element.querySelector('.e-summarycontent');
            if (!isNullOrUndefined(footer)) {
                EventHandler.remove(footer, 'scroll', this.onContentScroll);
            }
        }
    }

    /**
     * Function to get the scrollbar width of the browser.
     *
     * @returns {number} The width of the scrollbar in pixels.
     * @hidden
     */
    public static getScrollBarWidth(): number {
        return getScrollBarWidth();
    }
}

/**
 * @hidden
 */
export interface ScrollCss {
    padding?: string;
    border?: string;
}
