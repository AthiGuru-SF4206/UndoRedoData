import { EventHandler, Browser, isNullOrUndefined } from '@syncfusion/ej2-base';
import { debounce } from '@syncfusion/ej2-base';
import { SentinelInfo, SentinelType } from './virtual-scroll';
import { InterSection, VirtualInfo } from './interfaces';
import { parentsUntil } from './util';
export type ScrollDirection = 'up' | 'down' | 'right' | 'left';
/**
 * InterSectionObserver - class watch whether it enters the viewport.
 *
 * @hidden
 */
export class InterSectionObserver {
    private containerRect: ClientRect;
    private element: HTMLElement;
    private fromWheel: boolean = false;
    private touchMove: boolean = false;
    public PreventAdjustTable: string;
    /** @hidden */
    public options: InterSection = {};
    public blazorActiveKey: string;
    public isWheelScrolling: boolean = false;
    public isTouchScrolling: boolean = false;
    public sentinelInfo: SentinelInfo = {
        'up': {
            check: (rect: ClientRect, info: SentinelType) => {
                const top: number = rect.top - this.containerRect.top;
                const bottom: number = this.containerRect.bottom > rect.bottom ? this.containerRect.bottom - rect.bottom : 0;
                info.entered = top >= 0;
                return top + (this.options.pageHeight / 2) >= 0 || (bottom > 0 && rect.bottom > 0);
            },
            axis: 'Y'
        },
        'down': {
            check: (rect: ClientRect, info: SentinelType) => {
                const bottom: number = rect.bottom;
                info.entered = rect.bottom <= this.containerRect.bottom;
                return ((bottom - this.containerRect.top) - (this.options.pageHeight / 2)) <= this.options.pageHeight / 2;
            }, axis: 'Y'
        },
        'right': {
            check: (rect: ClientRect, info: SentinelType) => {
                const right: number = rect.right;
                info.entered = right < this.containerRect.right;
                return right - this.containerRect.width <= this.containerRect.right;
            }, axis: 'X'
        },
        'left': {
            check: (rect: ClientRect, info: SentinelType) => {
                const left: number = rect.left;
                info.entered = left > 0;
                return left + this.containerRect.width >= this.containerRect.left;
            }, axis: 'X'
        }
    };
    constructor(element: HTMLElement, options: InterSection) {
        this.element = element;
        this.options = options;
    }

    public observe(callback: Function, onEnterCallback: Function): void {
        this.options.virtualScrollHandler = this.virtualScrollHandler(callback, onEnterCallback);
        this.containerRect = this.options.container.getBoundingClientRect();
        EventHandler.add(this.options.container, 'wheel', () => {
            this.fromWheel = true;
            this.isWheelScrolling = true;
            this.isTouchScrolling = false;
            // Add your additional property here if needed
            return true;
        }, this);
        EventHandler.add(this.options.container, 'touchstart', () => {
            this.isTouchScrolling = true;
            this.isWheelScrolling = false;
            // Add your additional property here if needed
            return true;
        }, this);
        EventHandler.add(this.options.container, 'scroll', this.options.virtualScrollHandler, this);
        if (!isNullOrUndefined(parentsUntil(this.element, 'e-gridcontent')) && !isNullOrUndefined(parentsUntil(this.element, 'e-gridcontent').querySelector('.e-movablescrollbar'))) {
            EventHandler.add(parentsUntil(this.element, 'e-gridcontent').querySelector('.e-movablescrollbar'), 'scroll', this.options.virtualScrollHandler, this);
        }
    }

    public disconnect(): void {
        this.containerRect = this.options.container.getBoundingClientRect();
        EventHandler.remove(this.options.container, 'wheel', () => {
            this.fromWheel = true;
            this.isWheelScrolling = true;
            this.isTouchScrolling = false;
            // Add your additional property here if needed
            return true;
        });
        EventHandler.add(this.options.container, 'touchstart', () => {
            this.isTouchScrolling = true;
            this.isWheelScrolling = false;
            // Add your additional property here if needed
            return true;
        }, this);
        EventHandler.remove(this.options.container, 'scroll', this.options.virtualScrollHandler);
        if (!isNullOrUndefined(parentsUntil(this.element, 'e-gridcontent')) && !isNullOrUndefined(parentsUntil(this.element, 'e-gridcontent').querySelector('.e-movablescrollbar'))) {
            EventHandler.remove(parentsUntil(this.element, 'e-gridcontent').querySelector('.e-movablescrollbar'), 'scroll', this.options.virtualScrollHandler);
        }
    }

    public check(direction: ScrollDirection): boolean {
        const info: SentinelType = this.sentinelInfo[`${direction}`];
        return info.check(this.element.getBoundingClientRect(), info);
    }

    /**
     * Constants for scroll behavior configuration
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private SCROLL_CONSTANTS: any = {
        DEBOUNCE_DELAY_CHROME: 200,
        DEBOUNCE_DELAY_DEFAULT: 100,
        DEBOUNCE_DELAY_HORIZONTAL: 50,
        LIGHT_SCROLL_THRESHOLD: 20
    };

    /**
     * Creates debounced callback functions with appropriate delays
     *
     * @param {Function} callback - Original callback to debounce
     * @returns {{standard: Function, horizontal: Function}} Object containing debounced callbacks
     */
    private createDebouncedCallbacks(callback: Function): {
        standard: Function;
        horizontal: Function;
    } {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const delay: any = Browser.info.name === 'chrome'
            ? this.SCROLL_CONSTANTS.DEBOUNCE_DELAY_CHROME
            : this.SCROLL_CONSTANTS.DEBOUNCE_DELAY_DEFAULT;

        return {
            standard: debounce(callback, delay),
            horizontal: debounce(callback, this.SCROLL_CONSTANTS.DEBOUNCE_DELAY_HORIZONTAL)
        };
    }

    /**
     * Retrieves and normalizes scroll offset from target element
     * Handles Safari on macOS negative scroll bug
     *
     * @param {HTMLElement} target - Scroll container element
     * @returns {ScrollOffset} Normalized scroll offset
     */
    private getScrollOffset(target: HTMLElement): ScrollOffset {
        let top: number = target.scrollTop;
        const left: number = target.scrollLeft;

        // Fix Safari on macOS negative scroll position bug
        if (this.isSafariOnMacOS() && top < 0) {
            top = target.scrollTop = 0;
        }

        return { top, left };
    }

    /**
     * Detects if browser is Safari running on macOS
     *
     * @returns {boolean} True if Safari on macOS, false otherwise
     */
    private isSafariOnMacOS(): boolean {
        const userAgent: string = navigator.userAgent;
        const isMacOS: boolean = userAgent.indexOf('Mac OS') !== -1;
        const isSafari: boolean = /^((?!chrome|android).)*safari/i.test(userAgent);
        return isMacOS && isSafari;
    }

    /**
     * Calculates scroll direction based on current and previous positions
     *
     * @param {ScrollOffset} offset - Current scroll offset
     * @param {HTMLElement} target - Scroll container element
     * @returns {ScrollDirection} Calculated scroll direction
     */
    private calculateScrollDirection(
        offset: ScrollOffset,
        target: HTMLElement
    ): ScrollDirection {
        const isHorizontalScrollbar: boolean = target.classList.contains('e-movablescrollbar');
        if (isHorizontalScrollbar) {
            return this.getHorizontalDirection(offset.left);
        }

        // Determine primary direction (vertical or horizontal)
        const hasVerticalScroll: boolean = this.options.previousTop !== offset.top;
        const hasHorizontalScroll: boolean = this.options.previousLeft !== offset.left;

        if (hasVerticalScroll) {
            return this.getVerticalDirection(offset.top);
        } else if (hasHorizontalScroll) {
            return this.getHorizontalDirection(offset.left);
        }

        // Default to vertical direction if no scroll detected
        return this.getVerticalDirection(offset.top);
    }

    /**
     * Determines vertical scroll direction
     *
     * @param {number}currentTop - Current vertical scroll position
     * @returns {ScrollDirection} 'up' or 'down' direction
     */
    private getVerticalDirection(currentTop: number): ScrollDirection {
        return this.options.previousTop < currentTop ? 'down' : 'up';
    }

    /**
     * Determines horizontal scroll direction
     *
     * @param {number}currentLeft - Current horizontal scroll position
     * @returns {ScrollDirection} 'left' or 'right' direction
     */
    private getHorizontalDirection(currentLeft: number): ScrollDirection {
        return this.options.previousLeft < currentLeft ? 'right' : 'left';
    }

    /**
     * Updates internal scroll tracking state
     *
     * @param {ScrollOffset} offset - Current scroll offset
     * @param {ScrollDirection} direction - Current scroll direction
     * @returns {void}
     */
    private updateScrollTracking(offset: ScrollOffset, direction: ScrollDirection): void {
        const isHorizontalMove: boolean = direction === 'left' || direction === 'right';
        // Update PreventAdjustTable for scroll optimization
        this.PreventAdjustTable = this.calculatePreventAdjustTable(offset, direction);
        // Update previous scroll positions
        if (!isHorizontalMove) {
            this.options.previousTop = offset.top;
        }
        this.options.previousLeft = offset.left;
    }

    /**
     * Calculates PreventAdjustTable value for scroll optimization
     *
     * @param {ScrollOffset} offset - Current scroll offset
     * @param {ScrollDirection} direction - Current scroll direction
     * @returns {string} 'horizontal' or direction value
     */
    private calculatePreventAdjustTable(
        offset: ScrollOffset,
        direction: ScrollDirection
    ): string {
        let scrollDirection: string = direction;
        const isHorizontalMove: boolean = direction === 'left' || direction === 'right';
        scrollDirection = isHorizontalMove ? 'horizontal'
            : ((this.options.previousTop === offset.top && offset.left === this.options.previousLeft)
                ? 'horizontal' : direction);
        return scrollDirection;
    }

    /**
     * Checks if the given axis is being tracked
     *
     * @param {string} axis - Axis to check ('X' or 'Y')
     * @returns {boolean} True if axis is tracked, false otherwise
     */
    private isAxisTracked(axis: string): boolean {
        return this.options.axes.indexOf(axis) !== -1;
    }

    /**
     * Checks if mask rows are present in the container
     *
     * @param {string} axis - Axis to check ('X' or 'Y')
     * @returns {boolean} True if mask rows exist for non-horizontal scroll
     */
    private hasMaskRows(axis: string): boolean {
        return axis !== 'X' &&
            this.options.container.querySelectorAll('.e-masked-row').length > 0;
    }

    /**
     * Checks if overscan is enabled for the given axis
     *
     * @param {string} axis - Axis to check ('X' or 'Y')
     * @returns {boolean} True if overscan enabled for non-horizontal scroll
     */
    private hasOverscan(axis: string): boolean {
        return axis !== 'X' && this.options.overscanCount > 0;
    }

    /**
     * Retrieves scroll threshold configuration
     *
     * @returns {ScrollThresholdConfig} Configuration object with scroll parameters
     */
    private getScrollThresholdConfig(): ScrollThresholdConfig {
        return {
            rowHeight: this.options.rowHeight,
            pageSize: this.options.pageSize,
            overscanCount: this.options.overscanCount,
            height: this.options.height
        };
    }

    /**
     * Gets view information with default fallback
     *
     * @returns {VirtualInfo} Virtual info with at minimum startIndex
     */
    private getViewInfo(): VirtualInfo {
        let viewInfo: VirtualInfo = this.options.viewInfo;
        if (isNullOrUndefined(viewInfo)) {
            viewInfo = { startIndex: 0 };
        }
        return viewInfo;
    }

    /**
     * Determines if current scroll is a light scroll (doesn't require full re-render)
     * Light scrolls occur when viewport movement is within acceptable thresholds
     *
     * @param {ScrollDirection} direction - Current scroll direction
     * @param {ScrollOffset} offset - Current scroll offset
     * @returns {boolean} True if light scroll, false if full re-render needed
     */
    private determineScrollType(
        direction: ScrollDirection,
        offset: ScrollOffset
    ): boolean {
        switch (direction) {
        case 'down':
            return this.isLightScrollDown(offset.top);
        case 'up':
            return this.isLightScrollUp(offset.top);
        case 'right':
            return this.isLightScrollHorizontal(offset.left, direction);
        case 'left':
            return this.isLightScrollHorizontal(offset.left, direction);
        default:
            return true;
        }
    }

    /**
     * Determines if downward scroll is light (within threshold)
     *
     * @param {number} scrollTop - Current vertical scroll position
     * @returns {boolean} True if light scroll, false if full re-render needed
     */
    private isLightScrollDown(scrollTop: number): boolean {
        if (!this.options.maskRowEnabled) {
            return true;
        }

        const config: ScrollThresholdConfig = this.getScrollThresholdConfig();
        const viewInfo: VirtualInfo = this.getViewInfo();
        const exactTopIndex: number = scrollTop / config.rowHeight;
        let inViewIndexCount: number = config.height / config.rowHeight;
        const exactEndIndex: number = exactTopIndex + inViewIndexCount;
        const pageSizeTotal: number = config.pageSize + (config.overscanCount * 2);
        const pageSizeBy2: number = pageSizeTotal / 2;
        const pageSizeBy4: number = pageSizeTotal / 4;
        const overscanRows: number = viewInfo.startIndex === 0 ? 0 : config.overscanCount;

        const threshold: number = (viewInfo.startIndex - overscanRows) +
            Math.round(pageSizeBy2 + pageSizeBy4);

        // Check if scroll exceeds threshold requiring full re-render
        if (exactEndIndex > threshold) {
            inViewIndexCount = Math.ceil(inViewIndexCount) - 1;
            const rowIndexDifference: number = Math.ceil(exactTopIndex) -
                (viewInfo.startIndex - overscanRows);

            // Full re-render needed if difference exceeds visible rows
            return rowIndexDifference < inViewIndexCount;
        }

        return true;
    }

    /**
     * Determines if upward scroll is light (within threshold)
     *
     * @param {number} scrollTop - Current vertical scroll position
     * @returns {boolean} True if light scroll, false if full re-render needed
     */
    private isLightScrollUp(scrollTop: number): boolean {
        if (!this.options.maskRowEnabled) {
            return true;
        }

        const config: ScrollThresholdConfig = this.getScrollThresholdConfig();
        const viewInfo: VirtualInfo = this.getViewInfo();
        const exactTopIndex: number = scrollTop / config.rowHeight;
        let inViewIndexCount: number = config.height / config.rowHeight;
        const pageSizeTotal: number = config.pageSize + (config.overscanCount * 2);
        const pageSizeBy2: number = pageSizeTotal / 2;
        const pageSizeBy4: number = pageSizeTotal / 4;
        const overscanRows: number = viewInfo.startIndex === 0 ? 0 : config.overscanCount;

        const threshold: number = (viewInfo.endIndex - overscanRows) -
            Math.round(pageSizeBy2 + pageSizeBy4);

        // Check if scroll is above threshold requiring full re-render
        if (exactTopIndex < threshold) {
            inViewIndexCount = Math.ceil(inViewIndexCount) - 1;
            const loadAtIndex: number = Math.round(
                ((viewInfo.startIndex * config.rowHeight) +
                    (pageSizeBy4 * config.rowHeight)) / config.rowHeight
            );

            // Full re-render needed if scrolled beyond load point
            return exactTopIndex >= loadAtIndex &&
                Math.ceil(exactTopIndex) <= viewInfo.startIndex;
        }

        return true;
    }

    /**
     * Determines if horizontal scroll is light (within threshold)
     *
     * @param {number} scrollLeft - Current horizontal scroll position
     * @param {ScrollDirection} direction - Scroll direction ('left' or 'right')
     * @returns {boolean} True if light scroll, false otherwise
     */
    private isLightScrollHorizontal(
        scrollLeft: number,
        direction: ScrollDirection
    ): boolean {
        const scrollDelta: number = direction === 'right'
            ? scrollLeft - this.options.previousLeft
            : this.options.previousLeft - scrollLeft;

        return scrollDelta < this.SCROLL_CONSTANTS.LIGHT_SCROLL_THRESHOLD;
    }

    private virtualScrollHandler(callback: Function, onEnterCallback: Function): Function {
        // prepare debounced callbacks once per handler creation
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const callbacks: any = this.createDebouncedCallbacks(callback);

        return (e: Event) => {

            const target: HTMLElement = e.target as HTMLElement;
            const scrollOffset: ScrollOffset = this.getScrollOffset(target);
            const scrollDirection: ScrollDirection = this.calculateScrollDirection(scrollOffset, target);
            // Update scroll tracking properties
            this.updateScrollTracking(scrollOffset, scrollDirection);

            // We are using the same below structure of if conditions in the getInfoFromView method when we enabled showVirtualMaskRow property.
            // Based on this structure only, we will decide whether we have to change the start and end index. So if we have scenario(while
            // scrolling) to change the start and end index(in getInfoFromView method), then here we have to set isLightScroll = false and
            // based on this isLightScroll value, we will adjust the table(in onEntered method) to show maskrow(if isLightScroll is false
            // then we show maskrow other we set the previously rendered translate itself so datarows will be shown(by using _this.translateMaskY value).

            // Determine if this is a light scroll (doesn't require full re-render)
            const isLightScroll: boolean = this.determineScrollType(scrollDirection, scrollOffset);

            // Get sentinel information for the current direction
            // eslint-disable-next-line security/detect-object-injection
            const currentSentinel: SentinelType = this.sentinelInfo[scrollDirection];

            // Early return if axis is not being tracked
            if (!this.isAxisTracked(currentSentinel.axis)) {
                return;
            }

            // Check viewport conditions
            const check: boolean = this.check(scrollDirection);
            const hasMaskRows: boolean = this.hasMaskRows(currentSentinel.axis);
            const hasOverscan: boolean = this.hasOverscan(currentSentinel.axis);

            if (currentSentinel.entered || ((isNullOrUndefined(this.blazorActiveKey)
                || this.blazorActiveKey === '') && !this.fromWheel && hasMaskRows)) {
                onEnterCallback(currentSentinel, isLightScroll, scrollDirection,
                                { top: scrollOffset.top, left: scrollOffset.left }, this.fromWheel, check);
            }
            const { top, left } = scrollOffset;
            if (check || hasMaskRows || hasOverscan) {
                const fn: Function = currentSentinel.axis === 'X' ? callbacks.horizontal : callbacks.standard;
                fn({
                    scrollDirection, sentinel: currentSentinel, offset: { top, left },
                    focusElement: document.activeElement, isWheelScroll: this.fromWheel
                });
            }
            if (!hasMaskRows) {
                this.fromWheel = false;
            }
        };
    }

    public setPageHeight(value: number): void {
        this.options.pageHeight = value;
    }
}

/**
 * Scroll offset information containing vertical and horizontal positions
 */
interface ScrollOffset {
    top: number;
    left: number;
}

/**
 * Configuration for light scroll threshold detection
 */
interface ScrollThresholdConfig {
    rowHeight: number;
    pageSize: number;
    overscanCount: number;
    height: number;
}
