import { Browser, isNullOrUndefined, EventHandler, KeyboardEventArgs } from '@syncfusion/ej2-base';
import { IGridOptions } from './interfaces';
import { SfGrid } from './sf-grid-fn';

/**
 * InfiniteScroll
 *
 * @returns {void} This method does not return a value.
 * @hidden
 */
export class InfiniteScroll {
    private parent: SfGrid;
    public infiniteScrollTop: number = 0;
    private infiniteDataRequested: boolean = false;
    public infiniteScrollDirection: string = '';
    public maxPage: number = 0;
    public infiniteInitialRender: boolean = true;
    private rowElements: Element[] = [];
    public isLazyChildLoad: boolean = false;
    private currentRowIndex: number = 0;
    //private keyInteraction: boolean = false;

    constructor(parent: SfGrid) {
        this.parent = parent;
        this.addKeydownListener();

    }

    public destroy(): void {
        this.removekeydownListener();
    }

    /**
     * Adds a keydown event listener to the parent element.
     * This listener triggers the `keydownHandler` method when a keydown event occurs.
     *
     * @private
     * @returns {void}
     */
    private addKeydownListener(): void {
        if (!this.parent.options.enableInfiniteScrolling){
            return;
        }
        EventHandler.add(this.parent.element, 'keydown', this.keydownHandler, this);
    }

    /**
     * Removes the keydown event listener to the parent element.
     * This listener triggers the `keydownHandler` method when a keydown event occurs.
     *
     * @private
     * @returns {void}
     */
    private removekeydownListener(): void {
        EventHandler.remove(this.parent.element, 'keydown', this.keydownHandler);
    }

    /**
     * Handles the keydown event for infinite scrolling in the grid.
     *
     * @param {KeyboardEventArgs} e - The keyboard event arguments.
     * @private
     * @returns {void}
     *
     * This method checks if the 'Tab' key is pressed and if the active element is the last cell in the grid.
     * If so, it triggers the infinite scroll handler to load more data.
     *
     * If the 'Tab' key is pressed along with the 'Shift' key and the active element is the first cell in the grid,
     * it also triggers the infinite scroll handler to load more data.
     */
    private keydownHandler(e: KeyboardEventArgs): void {
        const activeElement: Element = document.activeElement;
        const scrollElement: HTMLElement = this.parent.scrollModule.content;
        const previousLeft: number = this.parent.scrollModule.previousValues.left;
        if (e.key === 'Tab') {
            const lastCell: HTMLElement = this.getLastCell();
            const firstCell: HTMLElement = this.getFirstCell();
            // this.keyInteraction = true;
            if ((activeElement === lastCell) ||
                (this.parent.options.infiniteCacheMode && e.shiftKey && activeElement === firstCell)) {
                //this.keyInteraction = true;
                this.infiniteDataRequested = this.infiniteDataRequested ? false : this.infiniteDataRequested;
                this.infiniteScrollHandler(scrollElement, previousLeft, true);
            }
        }
    }

    /**
     * Retrieves the first cell element from the first row of the grid.
     *
     * @returns {HTMLElement} The first cell element of the first row.
     */
    public getFirstCell(): HTMLElement {
        const rows: Element[] = this.parent.getRows();
        const firstRow: HTMLTableRowElement = rows[0] as HTMLTableRowElement;
        return !isNullOrUndefined(firstRow) ? firstRow.children[0] as HTMLElement : null;
    }

    /**
     * Retrieves the last cell element from the last row of the grid.
     *
     * @returns {HTMLElement} The last cell element in the last row of the grid.
     */
    public getLastCell(): HTMLElement {
        const rows: Element[] = this.parent.getRows();
        const lastRow: HTMLTableRowElement = rows[rows.length - 1] as HTMLTableRowElement;
        return !isNullOrUndefined(lastRow) ? lastRow.cells[lastRow.cells.length - 1] as HTMLElement : null;
    }

    /**
     * Handles the data readiness for infinite scrolling in the grid.
     * @returns {void}
     */
    public infiniteOnDataReady(): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: InfiniteScroll = this;
        this.maxPage = Math.ceil(this.parent.options.totalItemCount / this.parent.options.pageSize);
        if (this.parent.options.allowGrouping && !this.parent.options.enableLazyLoading && this.parent.options.groupCount > 0) {
            this.maxPage = this.maxPage + 1;
        }
        const resetRequestTypes: Set<string> = new Set([
            'Refresh', 'Filtering', 'Sorting', 'sorting', 'Searching', 'Grouping', 'UnGrouping', 'Reorder', 'RowDragAndDrop',
            'refresh', 'filtering', 'searching', 'grouping', 'ungrouping', 'reorder', 'GroupExpandCollapse', null
        ]);

        if (resetRequestTypes.has(this.parent.options.requestType)) {
            _this.infiniteDataRequested = false;
        }
    }

    /**
     * Checks if the bottom of the scrollable element has been reached.
     *
     * @param {HTMLElement} scrollElement - The scrollable element to check.
     * @returns {boolean} - Returns `true` if the bottom of the scrollable element is reached, otherwise `false`.
     */
    private isBottomReached(scrollElement: HTMLElement): boolean {
        const offset: number = scrollElement.scrollHeight - scrollElement.scrollTop;
        const offsetRound: number = Math.round(offset);
        let offsetFloor: number = offset < scrollElement.clientHeight ? Math.ceil(offset) : Math.floor(offset);
        const noScrollBar: boolean = scrollElement.scrollHeight > scrollElement.clientHeight;
        if (offsetFloor > scrollElement.clientHeight) {
            offsetFloor = offsetFloor - 1;
        }
        return noScrollBar && (offsetFloor === scrollElement.clientHeight || offsetRound === scrollElement.clientHeight);
    }

    /**
     * Determines if the current page is a grouped page.
     *
     * @returns {boolean} - Returns `true` if grouping is allowed, lazy loading is disabled,
     *                      there are groups, the current page is greater than or equal to 1,
     *                      and it is not the initial render of infinite scrolling.
     */
    private isGroupCurrentPage(): boolean {
        return this.parent.options.allowGrouping && !this.parent.options.enableLazyLoading &&
            this.parent.options.groupCount > 0 && this.parent.options.currentPage >= 1 &&
            !this.infiniteInitialRender;
    }

    /**
     * Handles the infinite scroll event for the grid.
     *
     * @param {HTMLElement} scrollElement - The scrollable element of the grid.
     * @param {number} previousLeft - The previous horizontal scroll position.
     * @param {boolean} keyInteraction - Indicates if the scroll was triggered by keyboard interaction.
     * @private
     * @returns {void}
     */
    public infiniteScrollHandler(scrollElement: HTMLElement, previousLeft: number, keyInteraction: boolean): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: InfiniteScroll = this;
        const isLeftRightScroll: boolean = scrollElement.scrollLeft !== previousLeft;
        const infiniteContent: boolean = scrollElement.classList.contains('e-content');
        const delay: number = Browser.info.name === 'chrome' ? 200 : 100;
        //for less records calculation records less than equal to 90
        const isBlockSizeValid: boolean = (this.parent.options.pageSize * this.parent.options.infiniteInitialBlock)
                                <= this.parent.options.totalItemCount; // Ensure block size does not exceed total item count
        if (infiniteContent && !isLeftRightScroll && isBlockSizeValid) {
            const isBottom: boolean = this.isBottomReached(scrollElement);
            const scrollElementTop: number = Math.floor(scrollElement.scrollTop);
            const currentPage: number = this.parent.options.currentPage;
            if (!this.infiniteDataRequested) {
                if ((this.parent.getRows().length / this.parent.options.pageSize) === this.maxPage) {
                    return;
                }
                if (isBottom && !this.infiniteInitialRender && ((currentPage <=
                    this.maxPage - 1) || (this.parent.options.totalItemCount !== this.rowElements.length &&
                        !this.infiniteInitialRender && !this.parent.options.infiniteCacheMode))) {
                    const activeElement: Element = document.activeElement;
                    setTimeout(function (): void {
                        const isBottom1: boolean = _this.isBottomReached(scrollElement);
                        if (isBottom1 && (!keyInteraction || keyInteraction && activeElement === _this.getLastCell())) {
                            _this.parent.dotNetRef.invokeMethodAsync('LoadInfiniteData', {
                                requestType: 'InfiniteScrolling'
                            }, isBottom, false, false, null, null, 0, keyInteraction);
                        }
                    }, delay);
                    this.infiniteScrollTop = this.calculateScrollPosition('down');
                    this.infiniteScrollDirection = 'down';
                }
                else if (scrollElementTop === 0 && (currentPage > 1 ||
                    this.isGroupCurrentPage()) && this.parent.options.infiniteCacheMode) {
                    setTimeout(function (): void {
                        const scrollElement: HTMLElement = _this.parent.getContent() as HTMLElement;
                        if (scrollElement.scrollHeight !== scrollElement.clientHeight) {
                            _this.parent.dotNetRef.invokeMethodAsync('LoadInfiniteData', {
                                requestType: 'InfiniteScrolling'
                            }, isBottom, true, false, null, null, 0, keyInteraction);
                        }
                    }, delay);
                    this.infiniteScrollTop = this.calculateScrollPosition('up');
                    this.infiniteScrollDirection = 'up';
                }
            }
            this.infiniteDataRequested = isBottom && !this.infiniteInitialRender || scrollElementTop === 0;
            this.infiniteInitialRender = false;
        }
    }

    /**
     * Handles the infinite scroll functionality for lazy loading in the grid.
     *
     * @param {boolean} scrollDown - Indicates the direction of the scroll. `true` if scrolling down, `false` if scrolling up.
     *
     * This method checks the scroll direction and determines if new rows need to be loaded
     * based on the current scroll position. It identifies the rows that need to be loaded
     * and triggers the loading of data accordingly.
     *
     * - If scrolling down, it checks for rows with the class `e-lazyload-middle-down`.
     *
     * The method updates the `currentRowIndex` based on the identified rows and invokes
     * the `LoadInfiniteData` method to load the necessary data.
     *
     * @private
     * @returns {void}
     */
    public lazyLoadInfiniteScrollHandler(scrollDown: boolean): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: InfiniteScroll = this;
        const gridcontent: HTMLElement = this.parent.getContent();
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const downTrs: any = [].slice.call(gridcontent.getElementsByClassName('e-lazyload-middle-down'));
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const endTrs: any = [].slice.call(gridcontent.getElementsByClassName('e-lazyload-last-down'));
        let lazyLoadDown: boolean = false;
        let lazyLoadEnd: boolean = false;
        let middleRowIndex: number = 0;
        let tr: HTMLElement;
        let middleTr: HTMLElement;
        let endTr: HTMLElement;
        const prevRowIndex: number = this.currentRowIndex;
        const delay: number = Browser.info.name === 'chrome' ? 200 : 100;
        if (scrollDown && downTrs.length > 0) {
            const result: { rowEntered: boolean, rowIndex: number, row: HTMLElement } = this.findRowElementsInGrid(downTrs);
            lazyLoadDown = result.rowEntered;
            middleRowIndex = result.rowIndex;
            tr = middleTr = result.row;
            if (lazyLoadDown) {
                this.currentRowIndex = middleRowIndex;
            }
        }
        if (!scrollDown && endTrs.length > 0) {
            for (let i: number = 0; i < endTrs.length; i++) {
                const top: number = endTrs[parseInt(i.toString(), 10)].getBoundingClientRect().top;
                let endRowIndex: number = endTrs[parseInt(i.toString(), 10)].rowIndex;
                const scrollHeight: number = gridcontent.parentElement.scrollHeight;
                this.isLazyChildLoad = false;
                if (top > 0 && top < scrollHeight) {
                    tr = endTr = endTrs[parseInt(i.toString(), 10)];
                    lazyLoadEnd = true;
                    endRowIndex = (tr as HTMLTableRowElement).rowIndex;
                    if (lazyLoadEnd) {
                        this.currentRowIndex = endRowIndex;
                    }
                    break;
                }
            }
        }
        if (((scrollDown && lazyLoadDown) || (!scrollDown && lazyLoadEnd)) && this.currentRowIndex !== prevRowIndex &&
            !this.isLazyChildLoad) {
            const middleTrUid: string | null = !isNullOrUndefined(middleTr) ? middleTr.getAttribute('data-uid') : null;
            const endTrUid: string | null = !isNullOrUndefined(endTr) ? endTr.getAttribute('data-uid') : null;
            const childRequest: boolean = lazyLoadDown || lazyLoadEnd;
            setTimeout(function (): void {
                _this.parent.dotNetRef.invokeMethodAsync('LoadInfiniteData', {
                    requestType: 'InfiniteScrolling'
                }, false, false, childRequest, middleTrUid, endTrUid, middleRowIndex, false);
            }, delay);
            this.isLazyChildLoad = true;
        }
    }

    /**
     * Finds the first row element in the lazy load grouped grid that meets a specific condition.
     *
     * @param {HTMLElement[]} rows - An array of HTML row elements to search through.
     * @returns {Object} An object containing:
     *  - `rowEntered` {boolean}: Indicates if a row meeting the condition was found.
     *  - `rowIndex` {number}: The index of the row that meets the condition.
     *  - `row` {HTMLElement | null}: The row element that meets the condition, or null if none found.
     */
    private findRowElementsInGrid(rows: HTMLElement[]): any {
        let rowEntered: boolean = false;
        let row: HTMLElement | null = null;
        let rowIndex: number = 0;
        for (const currentRow of rows) {
            const currentRowIndex: number = (currentRow as HTMLTableRowElement).rowIndex;
            if (this.isRowEnteredInGrid(currentRowIndex)) {
                rowEntered = true;
                row = currentRow;
                rowIndex = currentRowIndex;
                break; // Exit the loop early
            }
        }
        return { rowEntered, rowIndex, row };
    }

    /**
     * Checks if a row with the given index is currently visible within the grid's viewport.
     *
     * @param {number} index - The index of the row to check.
     * @returns {boolean} - Returns `true` if the row is visible within the grid's viewport, otherwise `false`.
     */
    private isRowEnteredInGrid(index: number): boolean {
        const content: HTMLElement = this.parent.getContent();
        const rowHeight: number = this.parent.getRowHeight();
        const startIndex: number = content.scrollTop / rowHeight;
        const endIndex: number = startIndex + (content.offsetHeight / rowHeight);
        return index > startIndex && index < endIndex;
    }

    /**
     * Calculates the scroll position based on the given direction.
     *
     * @param {string} direction - The direction of the scroll, either 'up' or 'down'.
     * @returns {number} - The calculated scroll position.
     */
    private calculateScrollPosition(direction: string): number {
        let scrollTop: number = 0;
        const scrollElement: HTMLDivElement = <HTMLDivElement>this.parent.getContent();
        const gridRowHeight: number = this.parent.getRowHeight();
        const gridPageSize: number = this.parent.options.pageSize;
        if (direction === 'down') {
            if (this.parent.options.allowGrouping && !isNullOrUndefined(this.parent.options.initGroupingField) &&
                this.parent.options.initGroupingField.length > 0 && !this.parent.options.infiniteCacheMode) {
                let groupRowHeight: number = 0;
                const contentTable: HTMLElement = this.parent.getContentTable() as HTMLElement;
                const captionRows: NodeListOf<Element> = contentTable.querySelectorAll('tr:not(.e-row)');
                groupRowHeight = captionRows.length * gridRowHeight;
                scrollTop += groupRowHeight;
            }
            else {
                let pageSizeMaxBlock: number = gridPageSize * (this.parent.options.infiniteMaxBlocks - 1);
                pageSizeMaxBlock = pageSizeMaxBlock === 0 ? gridPageSize : pageSizeMaxBlock;
                let currentViewRowCount: number = 0;
                let i: number = 0;
                while (currentViewRowCount < scrollElement.offsetHeight) {
                    i++;
                    currentViewRowCount = i * gridRowHeight;
                }
                i -= 1;
                scrollTop += (pageSizeMaxBlock - i) * gridRowHeight;
            }

        }
        else if (direction === 'up') {
            scrollTop += gridPageSize * gridRowHeight;
        }
        return scrollTop;
    }

    /**
     * Resets the infinite scroll positions for various grid actions.
     * @returns {void}
     */
    public resetInfniniteScrollPositions(): void {
        const scrollElement: HTMLDivElement = <HTMLDivElement>this.parent.getContent();
        const gridOptions: IGridOptions = this.parent.options;
        if (this.infiniteInitialRender) {
            this.maxPage = Math.ceil(gridOptions.totalItemCount / gridOptions.pageSize);
        }
        const requestTypes: Set<string> = new Set([
            'Refresh', 'Filtering', 'ClearFiltering', 'Sorting', 'sorting', 'Searching',
            'Grouping', 'UnGrouping', 'Reorder', 'refresh', 'filtering', 'searching',
            'grouping', 'ungrouping', 'reorder', 'GroupExpandCollapse', 'GroupExpandCollapseAll', null
        ]);
        if (requestTypes.has(gridOptions.requestType)) {
            this.infiniteInitialRender = true;
            scrollElement.scrollTop = 0;
            this.infiniteScrollDirection = '';
        }
        if (gridOptions.requestType === 'Delete') {
            const reachedBottom: boolean = scrollElement.scrollTop + scrollElement.offsetHeight >= scrollElement.scrollHeight;
            this.infiniteInitialRender = reachedBottom ? true : this.infiniteInitialRender;
        }
        if (this.infiniteScrollDirection === 'down' || this.infiniteScrollDirection === 'up') {
            this.infiniteDataRequested = false;
            if (((gridOptions.currentPage <= (this.maxPage - 1)) || scrollElement.scrollTop === 0) && gridOptions.infiniteCacheMode && this.parent.options.requestType === 'InfiniteScrolling') {
                if (this.isMacOsSafariVersion('17.4')) {
                    // eslint-disable-next-line @typescript-eslint/no-this-alias
                    const _this: InfiniteScroll = this;
                    setTimeout(function (): void {
                        scrollElement.scrollTop = _this.infiniteScrollTop;
                    }, 500);
                } else {
                    scrollElement.scrollTop = this.infiniteScrollTop;
                }
            }
        }
        if (this.isLazyChildLoad) {
            this.isLazyChildLoad = false;
        }
    }

    /**
     * Checks if the current browser is Safari on macOS and if its version is greater than or equal to the specified minimum version.
     *
     * @param {string} minVersion - The minimum Safari version required, in the format "major.minor".
     * @returns {boolean} - Returns `true` if the browser is Safari on macOS and its version is greater than or equal to the specified minimum version, otherwise `false`.
     */
    private isMacOsSafariVersion(minVersion: string): boolean {
        const userAgent: string = navigator.userAgent;
        const isMacOS: boolean = userAgent.indexOf('Mac OS') !== -1; // check whether it is on macOS
        const isSafariBrowser: boolean = /^((?!chrome|android).)*safari/i.test(userAgent); // Check the browser is Safari

        if (isMacOS && isSafariBrowser) {
            // Extract Safari version number from the user agent
            const safariVersionMatch: RegExpMatchArray | null = userAgent.match(/Version\/(\d+)\.(\d+)/);
            if (safariVersionMatch) {
                const currentMainVersion: number = parseInt(safariVersionMatch[1], 10);
                const currentSubVersion: number = parseInt(safariVersionMatch[2], 10);
                // Splitting the main version and the sub version
                const [minMainVersion, minSubVersion]: number[] = minVersion.split('.').map(Number);
                // Comparing versions
                if (currentMainVersion > minMainVersion ||
                    (currentMainVersion === minMainVersion && currentSubVersion >= minSubVersion)) {
                    return true;
                }
            }
        }
        return false;
    }
}
