import { formatUnit, Browser, isNullOrUndefined, EventHandler, KeyboardEventArgs } from '@syncfusion/ej2-base';
import { VirtualInfo, InterSection, Column } from './interfaces';
import { InterSectionObserver } from './intersection-observer';
import { isGroupAdaptive, getScrollBarWidth, parentsUntil } from './util';
import { SfGrid } from './sf-grid-fn';

/**
 * VirtualContentRenderer
 *
 * @returns {void} This method does not return a value.
 * @hidden
 */
export class VirtualContentRenderer {
    private parent: SfGrid;
    private count: number;
    private maxPage: number;
    private maxBlock: number;
    public observer: InterSectionObserver;
    private preStartIndex: number = 0;
    private preEndIndex: number;
    private preventEvent: boolean = false;
    private actions: string[] = ['filtering', 'clearfiltering', 'searching', 'grouping', 'ungrouping', 'Filtering', 'ClearFiltering', 'Searching', 'Grouping', 'Ungrouping', 'UnGrouping'];
    private content: HTMLElement;
    private offsets: { [x: number]: number } = {};
    private tmpOffsets: { [x: number]: number } = {};
    private offsetKeys: string[] = [];
    private prevInfo: VirtualInfo;
    private currentInfo: VirtualInfo = {};
    private contentPanel: Element;
    private isScrollByNavigation: boolean;
    private nextRowToNavigate: number = 0;
    private isScrollFromFocus: boolean;
    private translateMaskY: number;
    private translateMaskX: number;
    private movableTranslateX: number;
    private movableTranslateY: number;
    private isHeaderNavigated: boolean;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private scrollTimer: any;

    /** @hidden */
    public virtualEle: VirtualElementHandler;
    /** @hidden */
    public activeKey: string;
    /** @hidden */
    public rowIndex: number;
    /** @hidden */
    public requestType: string;
    /** @hidden */
    public startColIndex: number;
    /** @hidden */
    public endColIndex: number;
    /** @hidden */
    public vHelper: VirtualHelper;
    /** @hidden */
    public header: VirtualHeaderRenderer;
    /** @hidden */
    public startIndex: number = 0;
    /** @hidden */
    public movableColumnIndex: number;
    /** @hidden */
    public selectedCellNavigation: number = -1;
    /** @hidden */
    public selectedRowNavigation: number = -1;
    /** @hidden */
    public selectedRowIndex: number = -1;
    /** @hidden */
    public focusColumnIndex: number = -1;
    /** @hidden */
    public isScrollIntoview: boolean = false;
    /** @hidden */
    public scrollInfo: VirtualInfo = {};
    /** @hidden */
    public isScrollByFocus: boolean;
    /** @hidden */
    public isLastCell: boolean;
    /** @hidden */
    public frozenMidScroll: boolean = false;
    /** @hidden */
    public focusFromPager: boolean = false;
    /** @hidden */
    public keyCombination: string = '';


    constructor(parent: SfGrid) {
        this.parent = parent;
        this.contentPanel = this.parent.element.querySelector('.e-gridcontent');
        this.vHelper = new VirtualHelper(parent);
        this.virtualEle = new VirtualElementHandler(parent);
        this.addEventListener();
    }

    /**
     * Retrieves the header content panel element of the grid.
     *
     * @returns {Element} The header content panel element.
     */
    public getPanel(): Element {
        return this.contentPanel;
    }

    /**
     * Retrieves the header table element within the grid.
     *
     * @returns {Element} The header table element.
     */
    public getTable(): Element {
        return this.contentPanel.querySelector('.e-table');
    }

    /**
     * Renders and initializes the virtual table with intersection observer
     * Sets up scroll event handling and virtual element configuration
     *
     * @returns {number} The calculated row height for the grid
     * @throws {Error} If required elements are not found in DOM
     */
    public renderTable(): number {
        this.header = this.parent.virtualHeaderModule;
        this.virtualEle.table = this.getTable() as HTMLElement;
        this.virtualEle.content = this.content = this.getPanel().querySelector('.e-content') as HTMLElement;
        // Render virtual wrapper with parent height
        const gridHeight: number = Number(this.parent.options.height);
        this.virtualEle.renderWrapper(gridHeight);
        this.virtualEle.renderPlaceHolder();
        const content: HTMLElement = this.content;
        // Cache options to avoid repeated property access and improve readability
        const { enableColumnVirtualization, totalItemCount, overscanCount,
            enableVirtualMaskRow, height, pageSize } = this.parent.options;
        const rowHeight: number = this.parent.getRowHeight();
        const observerConfig: InterSection = {
            container: content,
            pageHeight: this.getBlockHeight() * 2,
            debounceEvent: true,
            axes: enableColumnVirtualization ? ['X', 'Y'] : ['Y'],
            totalItems: totalItemCount,
            overscanCount: overscanCount,
            pageSize: pageSize,
            rowHeight: rowHeight,
            maskRowEnabled: enableVirtualMaskRow,
            height: height.toString().indexOf('%') < 0 ? this.content.getBoundingClientRect().height :
                this.parent.element.getBoundingClientRect().height,
            previousTop: 0,
            previousLeft: 0
        };
        this.observer = new InterSectionObserver(this.virtualEle.wrapper, observerConfig);
        this.bindScrollEvent();
        return rowHeight;
    }

    private addEventListener(): void {
        const firstElementChild: Element = this.getPanel().firstElementChild as Element;
        firstElementChild.scrollTop = 0;
        firstElementChild.scrollLeft = 0;
        EventHandler.add(this.parent.element, 'keydown', this.keyDownHandler, this);
    }

    public removeEventListener(): void {
        if (!isNullOrUndefined(this.observer)) {
            this.observer.disconnect();
        }
        EventHandler.remove(this.parent.element, 'keydown', this.keyDownHandler);
    }

    private handleScrollEnd(virtualRefreshArgs: object, translatey: number, isWheelScroll: boolean, isScrollByNavigation: boolean): void {
        if (!isWheelScroll && !isScrollByNavigation) {
            this.parent.dotNetRef.invokeMethodAsync('VirtualRefresh', virtualRefreshArgs, translatey, this.selectedRowIndex, this.isScrollIntoview, this.focusColumnIndex, this.frozenMidScroll, this.focusFromPager, false);
        }
        if (isScrollByNavigation) {
            this.isScrollByNavigation = false;
        }
    }

    public ensurePageSize(): void {
        const rowHeight: number = this.parent.getRowHeight();
        // Cache options to avoid repeated property access and improve readability
        const { overscanCount, height, pageSize } = this.parent.options;
        const viewportHeight: string | number = height.toString().indexOf('%') < 0 ? this.content.getBoundingClientRect().height :
            this.parent.element.getBoundingClientRect().height;

        // Calculate visible rows and buffer size for smooth scrolling
        const visibleRowCount: number = Math.floor(viewportHeight / rowHeight);
        const bufferRowCount: number = visibleRowCount * 2;

        // Determine optimal page size
        const totalSizeWithOverscan: number = pageSize + overscanCount;
        const optimalPageSize: number = totalSizeWithOverscan < bufferRowCount
            ? bufferRowCount
            : pageSize;

        // Calculate virtual table width for column range
        const startOffset: number = this.getColumnOffset(this.startColIndex - 1);
        const endOffset: number = this.getColumnOffset(this.endColIndex);
        const virtualTableWidth: number = endOffset - startOffset;
        // Build configuration payload for .NET interop
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const pageConfiguration: any = {
            pageSize: optimalPageSize,
            startColumnIndex: this.startColIndex,
            endColumnIndex: this.endColIndex,
            VTableWidth: virtualTableWidth.toString()
        };
        this.parent.dotNetRef.invokeMethodAsync('SetPageSizeAndCIndex', pageConfiguration);
        this.parent.options.pageSize = optimalPageSize;
        this.observer.options.pageHeight = this.getBlockHeight() * 2;
    }

    /**
     * Prevents focus changes during virtual scrolling by redirecting focus to the grid content container.
     * This method is called when wheel or touch scrolling is detected to avoid focus jumping
     * between individual cells during rapid scroll operations.
     *
     * When a row cell is focused during scroll, focus is transferred to the parent grid content
     * element with preventScroll option to maintain smooth scrolling behavior.
     *
     * @private
     * @returns {void}
     */
    private preventFocusWhileScroll(): void {
        const activeElement: Element | null = document.activeElement as Element;
        if (isNullOrUndefined(activeElement)) {
            return;
        }
        // Only proceed if the focused element is a row cell
        if (!activeElement.classList.contains('e-rowcell')) {
            return;
        }
        const gridContent: HTMLElement | null = parentsUntil(activeElement, 'e-content') as HTMLElement;
        if (!gridContent) {
            return;
        }

        // Verify we're within a valid grid element structure
        const gridElement: HTMLElement | null = parentsUntil(activeElement, 'e-grid') as HTMLElement;
        if (!gridElement) {
            return;
        }
        // Transfer focus to grid content container without triggering scroll
        // This prevents individual cell focus during rapid scroll operations
        gridContent.focus({ preventScroll: true });
    }

    /**
     * Applies auto-fit to virtualized columns that have autoFit enabled
     *
     * @private
     * @param {Column[]} columns - Array of virtualized columns
     * @returns {void}
     */
    private applyAutoFitToVirtualizedColumns(columns: Column[]): void {
        if (!columns || columns.length === 0) {
            return;
        }

        for (const column of columns) {
            if (column.autoFit) {
                this.parent.resizeModule.autoFitColumns(column.field);
            }
        }
    }

    private scrollListener(scrollArgs: ScrollArg): void {
        // Constants for timeout delays
        const SCROLL_DEBOUNCE_DELAY: number = 100;
        const IMMEDIATE_EXECUTION_DELAY: number = 0;
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: VirtualContentRenderer = this;
        const { enablePersistence, enableColumnVirtualization, enableVirtualMaskRow, enableVirtualization,
            enableRtl, frozenColumns, rowHeight, virtualizedColumns, pageSize, groupCount
        } = _this.parent.options;

        const parentRowHeight: number = _this.parent.getRowHeight();
        if (enablePersistence) {
            this.parent.scrollPosition = scrollArgs.offset;
        }
        if (this.preventEvent) { this.preventEvent = false; }

        const activeElement: Element | null = document.activeElement as Element;
        if (activeElement.classList.contains('e-grid') && this.isScrollByFocus && enableColumnVirtualization) {
            this.isScrollByFocus = false;
        }
        const info: SentinelType = scrollArgs.sentinel;
        const previousStartIndex: number = this.preStartIndex;
        const previousColumnIndexes: number[] = this.parent.getColumnIndexesInView();

        const viewInfo: VirtualInfo = this.observer.options.viewInfo =
            this.currentInfo = this.getInfoFromView(scrollArgs.scrollDirection, info, scrollArgs.offset);

        const horizontalTranslateValue: number = enableColumnVirtualization &&
            enableRtl ? -1 * this.getColumnOffset(viewInfo.columnIndexes[0] - 1) :
            this.getColumnOffset(viewInfo.columnIndexes[0] - 1);
        let isPreventFocusScroll: boolean = false;

        const hasColumnIndexChanged: boolean = this.parent.options.enableColumnVirtualization &&
            (JSON.stringify(previousColumnIndexes) !== JSON.stringify(viewInfo.columnIndexes));
        //Horizontal Scroll Handling
        if (enableColumnVirtualization && hasColumnIndexChanged) {
            // Apply auto-fit for virtualized columns if needed
            this.applyAutoFitToVirtualizedColumns(virtualizedColumns);

            const columnTranslateX: number = enableColumnVirtualization && enableRtl ? -1 *
                this.getColumnOffset(this.startColIndex - 1) : this.getColumnOffset(this.startColIndex - 1);
            const endOffset: number = this.getColumnOffset(this.endColIndex);
            const tableWidth: number = enableColumnVirtualization && enableRtl
                ? -1 * endOffset + columnTranslateX : endOffset - columnTranslateX;
            const verticalTranslateY: number = !isNullOrUndefined(viewInfo.endIndex) ?
                (viewInfo.endIndex - pageSize) * (rowHeight ? rowHeight : parentRowHeight) : 0;
            this.movableTranslateY = !isNullOrUndefined(this.movableTranslateY) ? this.movableTranslateY : 0;

            // Calculate translateY for virtual mask rows
            const maskTranslateY: number = (enableVirtualMaskRow && enableVirtualization)
                ? (frozenColumns ? this.movableTranslateY : this.translateMaskY + this.movableTranslateY)
                : 0;
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const virtualRefreshArgs: any = {
                requestType: 'virtualscroll',
                isHeaderNavigated: _this.isHeaderNavigated,
                selectedRowNavigation: _this.selectedRowNavigation,
                selectedCellNavigation: _this.selectedCellNavigation,
                isScrollByFocus: _this.isScrollByFocus,
                startColumnIndex: viewInfo.columnIndexes[0],
                endColumnIndex: viewInfo.columnIndexes[viewInfo.columnIndexes.length - 1],
                axis: 'X',
                VTablewidth: tableWidth.toString(),
                translateX: horizontalTranslateValue,
                translateY: maskTranslateY
            };
            clearTimeout(this.scrollTimer);
            this.scrollTimer = setTimeout(function (): void {
                _this.handleScrollEnd(virtualRefreshArgs, verticalTranslateY, scrollArgs.isWheelScroll, false);
            }, SCROLL_DEBOUNCE_DELAY);

            setTimeout(() => {
                _this.parent.dotNetRef.invokeMethodAsync('RemoveValidationPopup');
                if (scrollArgs.isWheelScroll) {
                    _this.parent.dotNetRef.invokeMethodAsync('VirtualRefresh',
                                                             virtualRefreshArgs, verticalTranslateY, _this.selectedRowIndex,
                                                             _this.isScrollIntoview,
                                                             _this.focusColumnIndex, false, false, false);
                }
                _this.isScrollByFocus = false;
            }, IMMEDIATE_EXECUTION_DELAY);
        }
        else {
            // Handle horizontal mask row adjustment when column indexes haven't changed
            const isHorizontalScroll: boolean = this.currentInfo.direction === 'left' || this.currentInfo.direction === 'right';
            if (isHorizontalScroll && enableVirtualMaskRow) {
                this.virtualEle.adjustTable(horizontalTranslateValue, this.translateMaskY, this.currentInfo.direction);
            }
        }
        const columnIndexesInView: number[] = enableColumnVirtualization ? viewInfo.columnIndexes : [];
        this.parent.setColumnIndexesInView(columnIndexesInView);

        if (this.isScrollByNavigation) {
            clearTimeout(this.scrollTimer);
            this.scrollTimer = setTimeout(function (): void {
                _this.handleScrollEnd(null, 0, null, true);
            }, SCROLL_DEBOUNCE_DELAY);
        } else {
            this.nextRowToNavigate = 0;
        }
        //Vertical Scroll Handling
        const hasRowIndexChanged: boolean = this.preStartIndex !== previousStartIndex;
        if (hasRowIndexChanged && enableVirtualization) {
            if (groupCount === 0 && (this.observer.isWheelScrolling || this.observer.isTouchScrolling)) {
                this.preventFocusWhileScroll();
                isPreventFocusScroll = true;
            }
            _this.parent.options.currentPage = viewInfo.currentPage;
            const isCalledFromScrollIntoView: boolean = _this.isScrollIntoview;
            const nextRowToFocus: number = this.nextRowToNavigate;
            const startIndex: number = viewInfo.endIndex - pageSize;
            const translateY: number = startIndex * (rowHeight ? rowHeight : parentRowHeight);

            // Build vertical refresh arguments
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const verticalRefreshArgs: any = {
                requestType: 'virtualscroll',
                nextRowToNavigate: nextRowToFocus,
                virtualStartIndex: startIndex,
                virtualEndIndex: viewInfo.endIndex,
                axis: 'Y',
                RHeight: parentRowHeight
            };
            setTimeout(() => {
                _this.parent.dotNetRef.invokeMethodAsync(
                    'VirtualRefresh',
                    verticalRefreshArgs,
                    translateY,
                    _this.selectedRowIndex,
                    isCalledFromScrollIntoView,
                    _this.focusColumnIndex,
                    false,
                    false,
                    isPreventFocusScroll
                );
            }, IMMEDIATE_EXECUTION_DELAY);
            _this.isScrollIntoview = false;
        }
        else if (this.shouldHandleRowSelection(previousStartIndex)) {
            _this.parent.dotNetRef.invokeMethodAsync('SelectRow',
                                                     _this.selectedRowIndex, _this.isScrollIntoview, _this.focusColumnIndex);
            _this.selectedRowIndex = -1;
            _this.isScrollIntoview = false;
        }
        //Update previous info for next scroll event
        this.prevInfo = viewInfo;
    }

    /**
     * Determines if row selection should be handled for scroll into view scenario
     *
     * @private
     * @param {number} previousStartIndex - Previous start index before scroll
     * @returns {boolean} True if row selection should be handled
     */
    private shouldHandleRowSelection(previousStartIndex: number): boolean {
        const isValidRowIndex: boolean = this.selectedRowIndex >= 0;
        const hasIndexNotChanged: boolean = this.preStartIndex === previousStartIndex;
        const isScrollIntoView: boolean = this.isScrollIntoview;
        const isVerticalScroll: boolean = this.currentInfo.direction === 'up' || this.currentInfo.direction === 'down';

        return isValidRowIndex && hasIndexNotChanged && isScrollIntoView && isVerticalScroll;
    }

    /**
     * Sets the virtual table width and applies translation transforms for column virtualization.
     * This method adjusts both header and content virtual elements when column indexes change
     * or when explicitly refreshed. Handles frozen columns and RTL scenarios.
     *
     * @public
     * @param {object} [args] - Optional configuration object
     * @param {boolean} [args.refresh] - Whether to force refresh regardless of column index changes
     * @param {string} [args.axis] - The scroll axis ('X' or 'Y')
     * @param {string} [args.direction] - The scroll direction (e.g., 'left', 'right')
     * @returns {void}
     */
    public setColVTableWidthAndTranslate(args?: { refresh: boolean, axis: string, direction: string }): void {
        const { enableColumnVirtualization, frozenColumns, enableVirtualization } = this.parent.options;
        // Determine if column indexes have changed or refresh is forced
        const hasColumnIndexChanged: boolean = enableColumnVirtualization && this.prevInfo &&
            (JSON.stringify(this.currentInfo.columnIndexes) !==
                JSON.stringify(this.prevInfo.columnIndexes));
        const shouldRefresh: boolean = args && args.refresh;
        if ((hasColumnIndexChanged) || (shouldRefresh)) {
            const translateX: number = this.getColumnOffset(this.startColIndex - 1);
            const virtualTableWidth: number = this.getColumnOffset(this.endColIndex) - translateX;
            const tableWidth: string = virtualTableWidth.toString();
            if (frozenColumns === 0) {
                this.header.virtualEle.setWrapperWidth(tableWidth);
                this.virtualEle.setWrapperWidth(tableWidth);
                this.parent.getContentTable().parentElement.style.width = tableWidth + 'px';
            }
            // Apply translation to header with direction
            const headerDirection: string = args && args.direction ? args.direction : '';
            this.header.virtualEle.adjustTable(this.movableTranslateX, 0, headerDirection);
            if (enableColumnVirtualization && args && args.axis === 'X') {
                if (!enableVirtualization) {
                    this.invokeAdjustTable(this.movableTranslateX, 0, args.direction);
                } else {
                    const existingTranslateY: string = this.virtualEle.extractTranslateY(this.virtualEle.wrapper.style.transform);
                    const translateXValue: number = parseInt(existingTranslateY.replace('px', ''), 10);
                    this.invokeAdjustTable(this.movableTranslateX, translateXValue, args.direction);
                }
            }
        }
    }

    private invokeAdjustTable(xValue: number, yValue: number, direction: string): void {
        this.virtualEle.adjustTable(xValue, yValue, direction);
    }

    public refreshOnDataChange(): void {
        const panelFirstElementChild: Element = this.getPanel().firstElementChild as Element;
        panelFirstElementChild.scrollTop = 0;
        panelFirstElementChild.scrollLeft = 0;
        if (this.parent.options.enableColumnVirtualization) {
            this.header.virtualEle.adjustTable(0, 0);
        }
        this.virtualEle.adjustTable(0, 0);
        this.refreshOffsets();
        this.refreshVirtualElement();
    }

    /**
     * Handles keyboard events for virtual scrolling navigation.
     * Tracks arrow key presses for scroll optimization and handles Ctrl+Home/End navigation.
     *
     * This method:
     * - Sets the active arrow key (ArrowUp/ArrowDown) on the observer for scroll optimization
     * - Detects Ctrl+Home and Ctrl+End key combinations for quick navigation
     * - Prevents default browser behavior for Ctrl+Home/End to enable custom handling
     *
     * @private
     * @param {KeyboardEventArgs} keyboardEvent - The keyboard event arguments
     * @returns {void}
     */
    private keyDownHandler(keyboardEvent: KeyboardEventArgs): void {
        const ARROW_DOWN: string = 'ArrowDown';
        const ARROW_UP: string = 'ArrowUp';
        const END_KEY: string = 'End';
        const HOME_KEY: string = 'Home';
        const CTRL_END: string = 'ctrlEnd';
        const CTRL_HOME: string = 'ctrlHome';
        const EMPTY_KEY: string = '';

        if (isNullOrUndefined(this.observer)) {
            return;
        }

        // Cache the pressed key to avoid repeated property access
        const pressedKey: string = keyboardEvent.key;
        // Track arrow key presses for scroll optimization
        const isArrowDown: boolean = pressedKey === ARROW_DOWN;
        const isArrowUp: boolean = pressedKey === ARROW_UP;
        const isArrowKey: boolean = isArrowDown || isArrowUp;
        this.observer.blazorActiveKey = isArrowKey ? pressedKey : EMPTY_KEY;

        // Handle Ctrl+Home and Ctrl+End navigation keys
        if (keyboardEvent.ctrlKey) {
            const isEndKey: boolean = pressedKey === END_KEY;
            const isHomeKey: boolean = pressedKey === HOME_KEY;
            const isNavigationKey: boolean = isEndKey || isHomeKey;

            if (isNavigationKey) {
                this.keyCombination = isEndKey ? CTRL_END : CTRL_HOME;
                keyboardEvent.preventDefault();
            }
        }
    }

    public columnVirtualizationKeyDownHandler(keyboardEvent: KeyboardEventArgs): void {
        // Constants for keys and CSS classes
        const TAB_KEY: string = 'Tab';
        const LEFT_FREEZE_CLASS: string = 'e-leftfreeze';
        const RIGHT_FREEZE_CLASS: string = 'e-rightfreeze';
        const ROW_CELL_CLASS: string = 'e-rowcell';
        const HEADER_CELL_CLASS: string = 'e-headercell';
        this.focusFromPager = false;
        // --- SECTION 1: Cache parent options and extract event target information ---
        const { frozenColumns, columns } = this.parent.options;
        const targetElement: HTMLElement = keyboardEvent.target as HTMLElement;
        const targetCell: Element = parentsUntil(targetElement as Element, ROW_CELL_CLASS);
        const targetHeader: Element = parentsUntil(targetElement, HEADER_CELL_CLASS);

        // --- SECTION 2: Calculate column index and determine column set ---
        const columnIndex: number = targetCell ? parseInt(targetCell.getAttribute('aria-colindex'), 10) - 1 : -1;
        const orderedFrozenColumns: Column[] = this.parent.getOrderedFrozenColumns();
        const gridColumns: Column[] = frozenColumns === 0 ? columns : orderedFrozenColumns;

        // --- SECTION 3: Check frozen column state and scroll requirements ---
        const scrollValue: number = Math.round(this.content.scrollLeft + this.content.clientWidth);
        const isFrozenElement: boolean = targetElement.classList.contains(LEFT_FREEZE_CLASS)
            || targetElement.classList.contains(RIGHT_FREEZE_CLASS);
        let updateScrollLeft: boolean = false;
        if (frozenColumns > 0 && isFrozenElement) {
            const currentCellIsLastFreeze: boolean = this.checkNextElementHasFreeze(targetElement);
            updateScrollLeft = currentCellIsLastFreeze;
        }

        // --- SECTION 4: Delegate to appropriate handler based on Tab direction ---
        const isForwardTab: boolean = keyboardEvent.key === TAB_KEY && !keyboardEvent.shiftKey;
        const isBackwardTab: boolean = keyboardEvent.key === TAB_KEY && keyboardEvent.shiftKey;
        if (isForwardTab) {
            this.handleForwardTabNavigation(targetElement, targetHeader, columnIndex,
                                            gridColumns,
                                            updateScrollLeft,
                                            scrollValue
            );
        }
        else if (isBackwardTab) {
            this.handleBackwardTabNavigation(targetElement, columnIndex, orderedFrozenColumns,
                                             columns,
                                             frozenColumns,
                                             updateScrollLeft,
                                             scrollValue
            );
        }
    }

    /**
     * Handles forward Tab key navigation for column virtualization.
     * Resets scroll position to start when reaching last column or navigating from frozen columns.
     *
     * @private
     * @param {HTMLElement} targetElement - The currently focused element
     * @param {Element} targetHeader - The header cell element if target is in header
     * @param {number} columnIndex - The column index of the target element
     * @param {Column[]} gridColumns - Array of grid columns
     * @param {boolean} updateScrollLeft - Whether to update scroll left position
     * @param {number} scrollValue - Current scroll position value
     * @returns {void}
     */
    private handleForwardTabNavigation(targetElement: HTMLElement, targetHeader: Element, columnIndex: number,
                                       gridColumns: Column[],
                                       updateScrollLeft: boolean,
                                       scrollValue: number
    ): void {
        const LEFT_FREEZE_CLASS: string = 'e-leftfreeze';
        const ROW_CLASS: string = 'e-row';
        const MAPPING_UID_ATTR: string = 'e-mappinguid';
        const ARIA_COL_INDEX_ATTR: string = 'aria-colindex';
        const ARIA_ROW_INDEX_ATTR: string = 'aria-rowindex';

        // Check if target is last header cell
        let lastHeaderCell: boolean = false;
        if (!isNullOrUndefined(targetHeader)) {
            const headerDiv: Element = targetHeader.querySelector(`[${MAPPING_UID_ATTR}]`) as Element;
            if (!isNullOrUndefined(headerDiv)) {
                const headerMappingUid: string = headerDiv.getAttribute(MAPPING_UID_ATTR);
                const lastColumnUid: string = gridColumns[gridColumns.length - 1].uid;
                lastHeaderCell = headerMappingUid === lastColumnUid;
            }
        }

        // Check if next element after target is pager
        const nextTargetIsPager: boolean = this.parent.options.allowPaging
            && targetElement.parentElement.nextElementSibling == null;

        // Determine if current column is last in grid
        const isLastColumn: boolean = columnIndex === (gridColumns.length - 1) && !nextTargetIsPager;
        const gridContent: HTMLElement = this.parent.getContent();
        // Check if scroll update is needed for frozen mid-scroll scenario
        const isScrollableFrozenScenario: boolean = updateScrollLeft
            && scrollValue <= gridContent.scrollWidth
            && gridContent.scrollLeft !== 0
            && targetElement.classList.contains(LEFT_FREEZE_CLASS);

        const needsScrollUpdate: boolean = this.frozenMidScroll = isScrollableFrozenScenario;

        // Apply scroll and navigation state updates after DOM settles
        setTimeout((): void => {
            const shouldResetScroll: boolean = isLastColumn || lastHeaderCell || needsScrollUpdate;

            if (shouldResetScroll) {
                // Reset scroll to start for wraparound or frozen column navigation
                gridContent.scrollLeft = 0;
                // Store navigation state for frozen mid-scroll scenario
                if (needsScrollUpdate && !isNullOrUndefined(targetElement)) {
                    const targetColIndex: number = parseInt(
                        targetElement.getAttribute(ARIA_COL_INDEX_ATTR),
                        10
                    );
                    this.selectedCellNavigation = targetColIndex;
                    const parentRow: Element = parentsUntil(targetElement, ROW_CLASS);
                    if (!isNullOrUndefined(parentRow)) {
                        const rowIndex: number = parseInt(
                            parentRow.getAttribute(ARIA_ROW_INDEX_ATTR),
                            10
                        ) - 1;
                        this.selectedRowNavigation = rowIndex;
                    }
                }
            }
        }, 30);
    }

    /**
     * Handles backward Tab (Shift+Tab) key navigation for column virtualization.
     * Scrolls to end when reaching first column or navigating to frozen columns.
     *
     * @private
     * @param {HTMLElement} targetElement - The currently focused element
     * @param {number} columnIndex - The column index of the target element
     * @param {Column[]} orderedFrozenColumns - Array of frozen columns in order
     * @param {Column[]} columns - Array of all grid columns
     * @param {number} frozenColumns - Count of frozen columns in the grid
     * @param {boolean} updateScrollLeft - Whether to update scroll left position
     * @param {number} scrollValue - Current scroll position value
     * @returns {void}
     */
    private handleBackwardTabNavigation(targetElement: HTMLElement, columnIndex: number, orderedFrozenColumns: Column[],
                                        columns: Column[],
                                        frozenColumns: number,
                                        updateScrollLeft: boolean,
                                        scrollValue: number
    ): void {
        // Constants
        const MOVABLE_SCROLLBAR_CLASS: string = 'e-movablescrollbar';
        const ROW_CLASS: string = 'e-row';
        const HEADER_CELL_DIV_CLASS: string = 'e-headercelldiv';
        const MAPPING_UID_ATTR: string = 'e-mappinguid';
        const ARIA_COL_INDEX_ATTR: string = 'aria-colindex';
        const ARIA_ROW_INDEX_ATTR: string = 'aria-rowindex';

        // Check if target is movable scrollbar (special pager case)
        const targetIsMovableScrollBar: boolean = this.parent.options.allowPaging
            && frozenColumns > 0
            && targetElement.classList.contains(MOVABLE_SCROLLBAR_CLASS);
        const gridContent: HTMLElement = this.parent.getContent();
        // Determine if current column is first in virtualized range
        const isFirstVirtualColumn: boolean = columnIndex === 0 || targetIsMovableScrollBar;

        // Check if scroll update is needed for frozen mid-scroll scenario
        const isScrollableFrozenScenario: boolean = updateScrollLeft
            && scrollValue < gridContent.scrollWidth;

        const needsScrollUpdate: boolean = this.frozenMidScroll = isScrollableFrozenScenario;

        // Apply scroll and navigation state updates after DOM settles
        setTimeout((): void => {
            const shouldScrollToEnd: boolean = isFirstVirtualColumn || needsScrollUpdate;

            if (shouldScrollToEnd) {
                // Set focus tracking flags for scroll restoration
                this.isScrollByFocus = true;
                this.isLastCell = true;
                this.selectedRowNavigation = 0;
                this.focusFromPager = targetIsMovableScrollBar;

                // Calculate last column index based on frozen column state
                const hasFrozenColumns: boolean = frozenColumns > 0;
                let selectedCellNavigation: number = hasFrozenColumns
                    ? orderedFrozenColumns.length - 1
                    : columns.length - 1;

                // Scroll to end to reveal last virtualized column
                gridContent.scrollLeft = gridContent.scrollWidth;

                // Update navigation state for frozen mid-scroll scenario
                if (needsScrollUpdate && !isNullOrUndefined(targetElement)) {
                    // Handle header cell scenario
                    const headerCellDiv: Element = targetElement.querySelector(`.${HEADER_CELL_DIV_CLASS}`);
                    if (!isNullOrUndefined(headerCellDiv)) {
                        const mappingUid: string = headerCellDiv.getAttribute(MAPPING_UID_ATTR);
                        const currentColIndex: number = orderedFrozenColumns.findIndex(
                            (col: Column): boolean => col.uid === mappingUid
                        );
                        selectedCellNavigation = currentColIndex;
                    }

                    // Handle data cell scenario
                    const parentRow: Element = parentsUntil(targetElement, ROW_CLASS);
                    if (!isNullOrUndefined(parentRow)) {
                        const cellColIndex: number = parseInt(
                            targetElement.getAttribute(ARIA_COL_INDEX_ATTR),
                            10
                        ) - 1;
                        selectedCellNavigation = cellColIndex;
                        const rowIndex: number = parseInt(
                            parentRow.getAttribute(ARIA_ROW_INDEX_ATTR),
                            10
                        ) - 1;
                        this.selectedRowNavigation = rowIndex;
                    }
                }
                // Store final cell navigation state
                this.selectedCellNavigation = selectedCellNavigation;
            }
        }, 30);
    }

    public checkNextElementHasFreeze(element: HTMLElement): boolean {
        // Get the next sibling element
        const nextElement: Element = element.nextElementSibling;
        // Check if the next sibling exists and has 'e-leftfreeze' in its class
        if (nextElement && nextElement.classList.contains('e-leftfreeze')) {
            return false;
        }
        return true;
    }

    public handleCellFocusAndNavigation(cell: HTMLElement, action: string, keyCombination?: string): void {
        // Navigation action constants
        const MOVE_RIGHT_CELL: string = 'MoveRightCell';
        const MOVE_LEFT_CELL: string = 'MoveLeftCell';
        const MOVE_DOWN_CELL: string = 'MoveDownCell';
        const MOVE_UP_CELL: string = 'MoveUpCell';

        // Key combination constants
        const ALT_W: string = 'AltW';
        const CTRL_HOME: string = 'CtrlHome';
        const CTRL_END: string = 'CtrlEnd';
        const HOME_KEY: string = 'Home';
        const END_KEY: string = 'End';

        // CSS class constants
        const HEADER_CELL_DIV_CLASS: string = 'e-headercelldiv';

        // Attribute constants
        const ARIA_COL_INDEX_ATTR: string = 'aria-colindex';
        const DATA_UID_ATTR: string = 'data-uid';
        const MAPPING_UID_ATTR: string = 'e-mappinguid';

        // Input validation
        if (!cell) {
            return;
        }
        const rowHeight: number = this.parent.getRowHeight();
        const content: HTMLElement = this.parent.getContent();
        const cellDOMRect: ClientRect = cell.getBoundingClientRect();
        const contentDOMRect: ClientRect = content.getBoundingClientRect();
        // Reset scroll tracking flags on observer
        if (this.observer) {
            this.observer.isWheelScrolling = false;
            this.observer.isTouchScrolling = false;
        }

        const { enableColumnVirtualization, columns, frozenColumns } = this.parent.options;

        // Boolean flags for navigation actions
        const isMoveRight: boolean = action === MOVE_RIGHT_CELL;
        const isMoveLeft: boolean = action === MOVE_LEFT_CELL;
        const isMoveDown: boolean = action === MOVE_DOWN_CELL;
        const isMoveUp: boolean = action === MOVE_UP_CELL;
        const isHorizontalNavigation: boolean = isMoveRight || isMoveLeft;
        const isVerticalNavigation: boolean = isMoveDown || isMoveUp;

        // --- SECTION 1: Handle cell navigation tracking for non-virtualized columns ---
        if (!enableColumnVirtualization && isHorizontalNavigation) {
            this.trackNonVirtualizedColumnNavigation(cell, isMoveRight, isMoveLeft, ARIA_COL_INDEX_ATTR);
        }

        // --- SECTION 2: Handle cell navigation tracking for virtualized columns ---
        if (enableColumnVirtualization && isHorizontalNavigation) {
            this.trackVirtualizedColumnNavigation(cell, frozenColumns, DATA_UID_ATTR, MAPPING_UID_ATTR
                , ARIA_COL_INDEX_ATTR, HEADER_CELL_DIV_CLASS);
        }

        // --- SECTION 3: Apply focus behavior based on navigation direction ---
        if (isVerticalNavigation) {
            // Vertical navigation: prevent scroll during focus to avoid browser auto-scroll
            cell.focus({ preventScroll: true });
        } else if (isHorizontalNavigation) {
            this.applyHorizontalNavigationFocus(cell, keyCombination, enableColumnVirtualization, columns);
        }

        // --- SECTION 4: Handle viewport scroll adjustments ---
        const scrollBarWidth: number = getScrollBarWidth();
        const visibleBottomEdge: number = contentDOMRect.top + contentDOMRect.height - scrollBarWidth;
        const visibleTopEdge: number = contentDOMRect.top + rowHeight;
        // Scroll down when cell is below viewport
        if (isMoveDown && cellDOMRect.bottom > visibleBottomEdge) {
            this.isScrollByNavigation = true;
            content.scrollTop += rowHeight;
        }
        // Scroll up when cell is above viewport
        else if (isMoveUp && cellDOMRect.bottom < visibleTopEdge) {
            this.isScrollFromFocus = true;
            this.isScrollByNavigation = true;
            content.scrollTop -= rowHeight;
        }

        // --- SECTION 5: Handle special key combinations ---
        const isCancelAction: boolean = keyCombination == null && action == null;
        const requiresSpecialHandling: boolean =
            keyCombination === ALT_W ||
            keyCombination === CTRL_HOME ||
            keyCombination === CTRL_END ||
            keyCombination === HOME_KEY ||
            keyCombination === END_KEY || isCancelAction;
        if (requiresSpecialHandling) {
            //keyCombination == null && action == null -> We get this when clicking Cancel in Add form
            const requiresScrollIntoView: boolean =
                keyCombination === ALT_W ||
                keyCombination === CTRL_HOME ||
                keyCombination === CTRL_END;
            if (requiresScrollIntoView) {
                cell.focus({ preventScroll: true });
                cell.scrollIntoView({ behavior: 'auto', block: 'nearest', inline: 'nearest' });
            }
            else {
                // Handle Home, End keys or cancel action
                cell.focus();
            }
        }
    }


    /**
     * Tracks cell navigation for non-virtualized columns
     *
     * @private
     * @param {HTMLElement} cell - The cell element being navigated
     * @param {boolean} isMoveRight - Whether navigation is moving right
     * @param {boolean} isMoveLeft - Whether navigation is moving left
     * @param {string} ariaColIndexAttr - The aria-colindex attribute name
     * @returns {void}
     */
    private trackNonVirtualizedColumnNavigation(cell: HTMLElement, isMoveRight: boolean,
                                                isMoveLeft: boolean, ariaColIndexAttr: string): void {
        const cellColIndex: number = Number(cell.getAttribute(ariaColIndexAttr)) - 1;
        if (this.selectedCellNavigation === -1) {
            this.selectedCellNavigation = cellColIndex;
        } else if (isMoveRight && this.selectedCellNavigation + 1 === cellColIndex) {
            this.selectedCellNavigation++;
        }
        else if (isMoveLeft && this.selectedCellNavigation - 1 === cellColIndex) {
            this.selectedCellNavigation--;
        }
    }

    /**
     * Tracks cell navigation for virtualized columns
     *
     * @param {HTMLElement} cell - The cell element being navigated
     * @param {number} frozenColumns - Count of frozen columns in the grid
     * @param {string} dataUidAttr - The data-uid attribute name
     * @param {string} mappingUidAttr - The e-mappinguid attribute name
     * @param {string} ariaColIndexAttr - The aria-colindex attribute name
     * @param {string} headerCellDivClass - The header cell div class name
     * @private
     * @returns {void}
     */
    private trackVirtualizedColumnNavigation(cell: HTMLElement, frozenColumns: number, dataUidAttr: string,
                                             mappingUidAttr: string, ariaColIndexAttr: string, headerCellDivClass: string
    ): void {
        const ROW_CLASS: string = 'e-row';
        const COLUMN_HEADER_CLASS: string = 'e-columnheader';

        let isHeaderCell: boolean = false;
        let parentRow: Element = parentsUntil(cell, ROW_CLASS);
        if (isNullOrUndefined(parentRow) && parentsUntil(cell, COLUMN_HEADER_CLASS)) {
            parentRow = parentsUntil(cell, COLUMN_HEADER_CLASS);
            isHeaderCell = true;
            this.selectedRowNavigation = 0;
        }
        let currentColIndex: number = 0;
        const gridColumns: Column[] = frozenColumns === 0 ?
            this.parent.getColumns() : this.parent.getOrderedFrozenColumns();
        // Get column index for header cell
        if (isHeaderCell) {
            const headerCellDiv: HTMLElement | null = cell.querySelector(headerCellDivClass);
            if (!isNullOrUndefined(cell) && !isNullOrUndefined(headerCellDiv)) {
                const mappingUid: string = headerCellDiv.getAttribute(mappingUidAttr) ||
                    cell.getAttribute(dataUidAttr);
                if (!isNullOrUndefined(mappingUid) && mappingUid !== '') {
                    const foundIndex: number = gridColumns.findIndex(
                        (col: Column): boolean => col.uid === mappingUid
                    );
                    currentColIndex = foundIndex >= 0 ? foundIndex : 0;
                }
            }
        }
        else {// Get column index for body cell
            const colIndexAttr: string | null = cell.getAttribute(ariaColIndexAttr);
            if (!isNullOrUndefined(cell)) {
                if (!isNullOrUndefined(colIndexAttr)) {
                    currentColIndex = Number(colIndexAttr) - 1;
                }
            }
        }
        this.selectedCellNavigation = currentColIndex;
    }

    /**
     * Applies focus for horizontal navigation (left/right)
     *
     * @private
     * @param {HTMLElement} cell - The cell element to apply focus to
     * @param {string} keyCombination - The key combination pressed (e.g., 'ShiftTab')
     * @param {boolean} enableColumnVirtualization - Whether column virtualization is enabled
     * @param {Column[]} columns - Array of grid columns
     * @returns {void}
     */
    private applyHorizontalNavigationFocus(cell: HTMLElement, keyCombination: string | undefined,
                                           enableColumnVirtualization: boolean,
                                           columns: Column[]
    ): void {
        const parentRow: Element | null = parentsUntil(cell, 'e-row');
        if (!isNullOrUndefined(parentRow)) {
            this.isHeaderNavigated = false;
            const rowIndexAttr: string | null = parentRow.getAttribute('aria-rowindex');
            this.selectedRowNavigation = rowIndexAttr
                ? Number(rowIndexAttr) - 1
                : 0;
        } else {
            this.isHeaderNavigated = true;
        }

        // Handle ShiftTab navigation on last cell in header
        if (enableColumnVirtualization && keyCombination === 'ShiftTab' && this.isLastCell && this.isHeaderNavigated) {
            this.selectedRowNavigation++;
            this.selectedCellNavigation = columns.length - 1;
            this.isLastCell = false;
        }
        this.isScrollByFocus = true;
        cell.focus();
    }

    private getInfoFromView(direction: string, info: SentinelType, offsets: Offsets): VirtualInfo {
        const virtualInfo: VirtualInfo = {
            direction: direction, sentinelInfo: info, offsets: offsets,
            startIndex: this.preStartIndex, endIndex: this.preEndIndex
        };
        // Cache parent options to avoid repeated property access
        const { pageSize, allowGrouping, overscanCount, groupCount, enableLazyLoading,
            height, enableVirtualMaskRow, totalItemCount, isRenderedFromTreeGrid
        } = this.parent.options;

        const viewportHeight: string | number = height.toString().indexOf('%') < 0 ? this.content.getBoundingClientRect().height :
            this.parent.element.getBoundingClientRect().height;
        virtualInfo.page = this.getPageFromTop(offsets.top + viewportHeight, virtualInfo);
        virtualInfo.blockIndexes = this.vHelper.getBlockIndexes(virtualInfo.page);
        virtualInfo.columnIndexes = info.axis === 'X' ? this.vHelper.getColumnIndexes() : this.parent.getColumnIndexesInView();

        // --- Row Start and End Index Calculation ---
        const rowHeight: number = this.parent.getRowHeight();
        const exactTopIndex: number = offsets.top / rowHeight;
        const noOfInViewIndexes: number = viewportHeight / rowHeight;
        const exactEndIndex: number = exactTopIndex + noOfInViewIndexes;

        // Calculate quarter and half page sizes (used for threshold calculations)
        let quarterPageSize: number = (pageSize) / 4;
        let halfPageSize: number = (pageSize) / 2;

        // Adjust for overscan in grouped scenarios
        const hasGroupingWithOverscan: boolean = overscanCount > 0 &&
            allowGrouping && groupCount > 0 && !enableLazyLoading &&
            overscanCount < this.parent.options.pageSize;
        if (hasGroupingWithOverscan) {
            const adjustedPageSize: number = pageSize + (overscanCount * 2);
            quarterPageSize = adjustedPageSize / 4;
            halfPageSize = adjustedPageSize / 2;
        }
        const totalCount: number = groupCount ? this.getVisibleGroupedRowCount() : this.count;
        // --- Handle Downward Scrolling ---
        if (virtualInfo.direction === 'down' && !this.isScrollFromFocus) {
            this.handleDownwardScroll(virtualInfo, exactTopIndex, exactEndIndex, noOfInViewIndexes,
                                      quarterPageSize,
                                      halfPageSize,
                                      totalCount,
                                      pageSize,
                                      overscanCount,
                                      totalItemCount,
                                      enableVirtualMaskRow
            );
        }
        // --- Handle Upward Scrolling ---
        else if (virtualInfo.direction === 'up') {
            this.handleUpwardScroll(virtualInfo, exactTopIndex, exactEndIndex, noOfInViewIndexes,
                                    quarterPageSize,
                                    halfPageSize,
                                    totalCount,
                                    rowHeight,
                                    pageSize,
                                    totalItemCount,
                                    enableVirtualMaskRow,
                                    isRenderedFromTreeGrid
            );
        }
        if (!enableVirtualMaskRow) {
            this.isScrollFromFocus = false;
            this.preStartIndex = this.startIndex = virtualInfo.startIndex;
            this.preEndIndex = virtualInfo.endIndex;
        }
        return virtualInfo;
    }

    /**
     * Handles downward scroll calculations and updates virtual info.
     * Modifies the virtualInfo object in place by calculating and updating:
     * - startIndex and endIndex based on scroll position
     * - currentPage based on end index
     * - Updates internal state (preStartIndex, preEndIndex, isScrollFromFocus)
     * - Updates navigation tracking (nextRowToNavigate)
     *
     * @private
     * @param {VirtualInfo} virtualInfo - Virtual information object to be updated with scroll calculations
     * @param {number} exactTopIndex - The exact row index at the top of the viewport
     * @param {number} exactEndIndex - The exact row index at the bottom of the viewport
     * @param {number} visibleRowCount - Number of rows visible in the current viewport
     * @param {number} quarterPageSize - One quarter of the page size for threshold calculations
     * @param {number} halfPageSize - Half of the page size for threshold calculations
     * @param {number} totalRowCount - Total number of rows in the grid
     * @param {number} pageSize - Number of rows per page
     * @param {number} overscanCount - Number of additional rows to render outside viewport
     * @param {number} totalItemCount - Total number of items in the data source
     * @param {boolean} enableVirtualMaskRow - Whether virtual mask row rendering is enabled
     * @returns {void}
     */
    private handleDownwardScroll(virtualInfo: VirtualInfo, exactTopIndex: number, exactEndIndex: number, visibleRowCount: number,
                                 quarterPageSize: number,
                                 halfPageSize: number,
                                 totalRowCount: number,
                                 pageSize: number,
                                 overscanCount: number,
                                 totalItemCount: number,
                                 enableVirtualMaskRow: boolean
    ): void {
        const suggestedStartIndex: number = Math.round(exactEndIndex) - Math.round(quarterPageSize);
        const overscanRowsRendered: number = this.preStartIndex === 0 ? 0 : overscanCount;

        // Handle virtual mask row scenario
        const thresholdForMaskRow: number = (virtualInfo.startIndex - overscanRowsRendered) +
            Math.round(halfPageSize + quarterPageSize);

        if (enableVirtualMaskRow && exactEndIndex > thresholdForMaskRow) {
            visibleRowCount = Math.ceil(visibleRowCount) - 1;
            const rowIndexDifference: number = Math.ceil(exactTopIndex) -
                (this.preStartIndex - overscanRowsRendered);

            if (rowIndexDifference >= visibleRowCount) {
                // Calculate new start and end indexes
                const calculatedStartIndex: number = Math.floor(exactTopIndex);
                virtualInfo.startIndex = calculatedStartIndex >= 0 ? calculatedStartIndex : 0;

                const calculatedEndIndex: number = virtualInfo.startIndex + pageSize;
                virtualInfo.endIndex = calculatedEndIndex < totalRowCount ? calculatedEndIndex : totalRowCount;

                // Adjust start index if end reached
                if (calculatedEndIndex >= totalRowCount) {
                    const adjustedStartIndex: number = virtualInfo.endIndex - pageSize;
                    virtualInfo.startIndex = adjustedStartIndex < 0 ? 0 : adjustedStartIndex;
                }

                virtualInfo.currentPage = Math.ceil(virtualInfo.endIndex / pageSize);
                this.preStartIndex = this.startIndex = virtualInfo.startIndex;
                this.preEndIndex = virtualInfo.endIndex;
            }

            this.isScrollFromFocus = false;
        }

        // Handle non-virtual-mask-row scenario
        const shouldUpdateIndexes: boolean = !enableVirtualMaskRow && isNullOrUndefined(virtualInfo.startIndex);
        const thresholdExceeded: boolean = exactEndIndex > (virtualInfo.startIndex + Math.round(halfPageSize + quarterPageSize));
        const notAtEnd: boolean = virtualInfo.endIndex !== totalRowCount;

        if (shouldUpdateIndexes || (thresholdExceeded && notAtEnd)) {
            virtualInfo.startIndex = suggestedStartIndex >= 0 ? Math.round(suggestedStartIndex) : 0;

            // Check if overscan end is reached
            const isOverscanEndReached: boolean = overscanCount > 0 &&
                overscanCount > pageSize &&
                virtualInfo.startIndex + pageSize >= totalItemCount;

            // Adjust start index based on exact top position
            if (virtualInfo.startIndex > exactTopIndex && !isOverscanEndReached) {
                virtualInfo.startIndex = Math.floor(exactTopIndex);
            }

            const calculatedEndIndex: number = virtualInfo.startIndex + pageSize;

            // Adjust start index if calculated end exceeds visible end
            if (calculatedEndIndex < exactEndIndex) {
                virtualInfo.startIndex = Math.ceil(exactEndIndex) - pageSize;
            }

            virtualInfo.endIndex = calculatedEndIndex < totalRowCount ? calculatedEndIndex : totalRowCount;

            // Final adjustment if at end
            if (calculatedEndIndex >= totalRowCount) {
                const adjustedStartIndex: number = virtualInfo.endIndex - pageSize;
                virtualInfo.startIndex = adjustedStartIndex > 0 ? adjustedStartIndex : 0;
            }

            virtualInfo.currentPage = Math.ceil(virtualInfo.endIndex / pageSize);
        }

        // Update navigation tracking
        const selectedIndexes: number[] = this.parent.getSelectedRowIndexes(true);
        this.nextRowToNavigate = selectedIndexes.length > 0 ? selectedIndexes[selectedIndexes.length - 1] - 1 : -1;
    }

    /**
     * Handles upward scroll calculations and updates virtual info.
     * Modifies the virtualInfo object in place by calculating and updating
     *
     * @private
     * @param {VirtualInfo} virtualInfo - Virtual information object to be updated with scroll calculations
     * @param {number} exactTopIndex - The exact row index at the top of the viewport
     * @param {number} exactEndIndex - The exact row index at the bottom of the viewport
     * @param {number} visibleRowCount - Number of rows visible in the current viewport
     * @param {number} quarterPageSize - One quarter of the page size for threshold calculations
     * @param {number} halfPageSize - Half of the page size for threshold calculations
     * @param {number} totalRowCount - Total number of rows in the grid
     * @param {number} rowHeight - Height of a single row in pixels
     * @param {number} pageSize - Number of rows per page
     * @param {number} totalItemCount - Total number of items in the data source
     * @param {boolean} enableVirtualMaskRow - Whether virtual mask row rendering is enabled
     * @param {boolean} isRenderedFromTreeGrid - Whether the grid is rendered as a tree grid
     * @returns {void}
     */
    private handleUpwardScroll(virtualInfo: VirtualInfo, exactTopIndex: number, exactEndIndex: number, visibleRowCount: number,
                               quarterPageSize: number,
                               halfPageSize: number,
                               totalRowCount: number,
                               rowHeight: number,
                               pageSize: number,
                               totalItemCount: number,
                               enableVirtualMaskRow: boolean,
                               isRenderedFromTreeGrid: boolean
    ): void {
        const hasIndexes: boolean = Boolean(virtualInfo.startIndex && virtualInfo.endIndex);

        if (!hasIndexes && !enableVirtualMaskRow) {
            return;
        }

        // Calculate load threshold
        const loadAtIndex: number = Math.round(
            ((virtualInfo.startIndex * rowHeight) + (quarterPageSize * rowHeight)) / rowHeight
        );

        // Handle virtual mask row scenario
        if (enableVirtualMaskRow) {
            visibleRowCount = Math.ceil(visibleRowCount);
            const shouldRecalculate: boolean = exactTopIndex < loadAtIndex ||
                Math.ceil(exactTopIndex) > this.preStartIndex;

            if (shouldRecalculate) {
                const ceiledTopIndex: number = Math.ceil(exactTopIndex);
                const calculatedStartIndex: number = ceiledTopIndex > 0 ? ceiledTopIndex : 0;

                // Calculate custom thresholds
                const customStartThreshold: number = totalRowCount - pageSize - halfPageSize;
                const endThreshold: number = totalRowCount - pageSize;
                const isNearEnd: boolean = endThreshold <= exactEndIndex && exactEndIndex <= totalRowCount;

                // Determine start index based on position
                if (exactTopIndex < totalRowCount &&
                    customStartThreshold <= exactTopIndex &&
                    !isNearEnd) {
                    virtualInfo.startIndex = calculatedStartIndex > 0
                        ? calculatedStartIndex - halfPageSize
                        : 0;
                } else {
                    const potentialStartIndex: number = calculatedStartIndex + pageSize;
                    virtualInfo.startIndex = calculatedStartIndex > 0
                        ? (potentialStartIndex > totalRowCount ? totalRowCount - pageSize : calculatedStartIndex)
                        : 0;
                }

                // Adjust for tree grid scenario
                if (virtualInfo.startIndex > 0 &&
                    isRenderedFromTreeGrid &&
                    virtualInfo.endIndex >= totalRowCount) {
                    // Keep current start index (no change)
                } else if (virtualInfo.startIndex > 0) {
                    virtualInfo.startIndex -= 1;
                }

                const calculatedEndIndex: number = virtualInfo.startIndex + pageSize;

                if (virtualInfo.startIndex <= 0) {
                    virtualInfo.endIndex = pageSize;
                } else {
                    virtualInfo.endIndex = calculatedEndIndex < totalRowCount
                        ? calculatedEndIndex
                        : totalRowCount;
                }

                // Adjust if one away from end
                if (virtualInfo.endIndex + 1 === totalRowCount) {
                    virtualInfo.endIndex = totalRowCount;
                }

                // Reset start index if negative or total items fit in page
                if (virtualInfo.startIndex < 0 || totalItemCount <= pageSize) {
                    virtualInfo.startIndex = 0;
                }

                // Adjust start index if at end
                if (virtualInfo.endIndex === totalRowCount) {
                    virtualInfo.startIndex = virtualInfo.endIndex - pageSize;
                }

                virtualInfo.currentPage = Math.ceil(virtualInfo.startIndex / pageSize);
                this.preStartIndex = this.startIndex = virtualInfo.startIndex;
                this.preEndIndex = virtualInfo.endIndex;
            }

            this.isScrollFromFocus = false;
        }

        // Handle non-virtual-mask-row scenario
        if (exactTopIndex < loadAtIndex && !enableVirtualMaskRow) {
            const indexAdjustment: number = quarterPageSize > visibleRowCount
                ? quarterPageSize
                : visibleRowCount + (visibleRowCount / 4);

            const calculatedEndIndex: number = Math.round(exactTopIndex + indexAdjustment);
            virtualInfo.endIndex = calculatedEndIndex < totalRowCount ? calculatedEndIndex : totalRowCount;

            const calculatedStartIndex: number = virtualInfo.endIndex - pageSize;
            virtualInfo.startIndex = calculatedStartIndex > 0 ? calculatedStartIndex : 0;

            // Adjust end index if start is negative
            if (calculatedStartIndex < 0) {
                virtualInfo.endIndex = pageSize;
            }

            virtualInfo.currentPage = Math.ceil(virtualInfo.startIndex / pageSize);
        }

        // Update navigation tracking
        const selectedRowIndexes: number[] = this.parent.getSelectedRowIndexes(true);
        const LAST_SELECTED_OFFSET: number = -1;
        this.nextRowToNavigate = selectedRowIndexes.length > 0
            ? selectedRowIndexes[0] + 1
            : 1 * LAST_SELECTED_OFFSET;
    }

    public onDataReady(): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: VirtualContentRenderer = this;
        const { totalItemCount, requestType, pageSize, isAdd, editMode,
            newRowPosition, enableVirtualMaskRow, enableVirtualization, rowHeight,
            enableLazyLoading, overscanCount, customizedOverScan, frozenColumns, enableColumnVirtualization
        } = this.parent.options;

        // --- SECTION 2: Update observer and basic state ---
        this.observer.options.totalItems = totalItemCount;
        this.count = totalItemCount;
        this.maxPage = Math.ceil(this.count / pageSize);

        // Rebind scroll event if not a virtual scroll request
        if (requestType !== 'virtualscroll') {
            this.bindScrollEvent();
        }

        // --- SECTION 3: Handle offset refresh for specific request types ---
        // Constants for request types that require offset refresh
        const REFRESH_TRIGGER_TYPES: string[] = [
            'Refresh', 'Filtering', 'ClearFiltering', 'Searching',
            'Grouping', 'UnGrouping', 'Reorder', 'RowDragAndDrop',
            'refresh', 'filtering', 'clearfiltering', 'searching',
            'grouping', 'ungrouping', 'reorder', 'GroupExpandCollapse',
            'InfiniteScrolling'
        ];

        if (REFRESH_TRIGGER_TYPES.indexOf(requestType) !== -1 || requestType == null){
            this.refreshOffsets();
        }

        // --- SECTION 4: Update virtual height and scroll module ---
        this.setVirtualHeight();
        this.parent.scrollModule.refresh();
        this.resetScrollPosition(requestType);
        this.setColVTableWidthAndTranslate();

        // --- SECTION 5: Handle bottom row addition in normal edit mode ---
        const isBottomNormalAdd: boolean = isAdd && editMode === 'Normal'
            && newRowPosition === 'Bottom';
        if (isBottomNormalAdd && enableVirtualization) {
            this.updateTransform(null, null, false, true);
        }

        // --- SECTION 6: Handle virtual mask row transformation ---
        const hasVirtualMaskWithVirtualization: boolean = enableVirtualMaskRow && enableVirtualization;
        const isNotGroupExpandCollapse: boolean = requestType !== 'GroupExpandCollapse';
        const shouldApplyMaskTransform: boolean = (hasVirtualMaskWithVirtualization || enableLazyLoading) && isNotGroupExpandCollapse;
        if (shouldApplyMaskTransform) {
            // Calculate grid-specific values
            const gridRowHeight: number = rowHeight;
            const gridPageSize: number = isBottomNormalAdd ? pageSize + 1 : pageSize;
            // Calculate start index with safety check
            const rawStartIndex: number = this.currentInfo.endIndex - gridPageSize;
            const startIndex: number = Math.max(0, Number.isFinite(Number(rawStartIndex)) ? Number(rawStartIndex) : 0);

            // Calculate Y translation value
            const effectiveRowHeight: number = gridRowHeight || this.parent.getRowHeight();
            let yValue: number = startIndex * effectiveRowHeight;

            // Reset Y value for specific conditions (NaN check or scroll at top with specific request types)
            const RESET_REQUEST_TYPES: string[] = ['Filtering', 'filtering', 'clearfiltering', 'ClearFiltering', 'Refresh'];
            const shouldResetYValue: boolean = isNaN(yValue) ||
                (this.content.scrollTop === 0 && RESET_REQUEST_TYPES.indexOf(requestType) !== -1);

            if (shouldResetYValue) {
                yValue = 0;
            }

            // Calculate overscan count based on current viewport state
            let gridOverScanCount: number = 0;
            if (overscanCount > 0) {
                const actualStartIndex: number = this.currentInfo.startIndex || 0;
                const viewportFirstDataRow: Element = this.content.querySelectorAll('.e-row:not(.e-masked-row)')[0];
                if (viewportFirstDataRow) {
                    const viewportStartIndex: number = parseInt(viewportFirstDataRow.getAttribute('aria-rowindex'), 10) - 1;
                    gridOverScanCount = this.isDefaultGrouping()
                        ? customizedOverScan
                        : actualStartIndex - viewportStartIndex;
                }
            }

            // Apply overscan adjustment
            const overscanRowHeight: number = gridRowHeight || this.parent.getRowHeight(startIndex !== 0);
            yValue -= (pageSize + gridOverScanCount) * overscanRowHeight;
            // Reset if no data
            if (this.count === 0) {
                yValue = 0;
            }
            // Handle horizontal translation for scroll into view
            if (this.isScrollIntoview && this.currentInfo.direction === 'down' && enableColumnVirtualization) {
                _this.translateMaskX = _this.translateMaskX === 0 ? this.movableTranslateX : _this.translateMaskX;
            }

            // Determine if transform should be applied now
            const isNotAtEnd: boolean = totalItemCount !== this.currentInfo.endIndex;
            const isScrollingUp: boolean = this.currentInfo.direction === 'up';
            const shouldApplyTransformNow: boolean = overscanCount === 0 || isNotAtEnd || isScrollingUp || isBottomNormalAdd;

            // Apply transform if conditions are met
            if (shouldApplyTransformNow) {
                setTimeout(() => {
                    _this.translateMaskY = yValue;
                    _this.translateMaskX = isNullOrUndefined(_this.translateMaskX) ? 0 : _this.translateMaskX;
                    const xTranslate: number = frozenColumns === 0 ? this.translateMaskX : 0;
                    const transformValue: string = `translate(${xTranslate}px, ${yValue}px)`;

                    this.virtualEle.wrapper.style.transform = transformValue;
                }, 0);
            }

        }
        // --- SECTION 7: Ensure previous info is initialized ---
        this.prevInfo = this.prevInfo || this.vHelper.getData();
    }

    /**
     * Set the virtual height for internal use.
     *
     * @hidden
     * @returns {void}
     */
    public setVirtualHeight(): void {
        const { columns, enableColumnVirtualization, totalItemCount, pageSize,
            groupCount, frozenColumns, enableVirtualMaskRow,
            visibleGroupedRowsCount, enableVirtualization
        } = this.parent.options;

        // Cache row height
        const baseRowHeight: number = this.parent.getRowHeight();
        // Determine width for virtual track; guard if no visible columns
        const visibleColumns: Column[] = (columns || []).filter((c: Column) => c && c.visible);
        const width: string = (enableColumnVirtualization && visibleColumns.length > 0)
            ? this.getColumnOffset(visibleColumns.length - 1) + 'px'
            : '100%';
        let virtualHeight: number = 0;
        const totalItemRowHeight: number = totalItemCount * baseRowHeight;
        const pageSizeRowHeight: number = pageSize * baseRowHeight;
        if (enableVirtualMaskRow) {
            if (frozenColumns > 0 && enableColumnVirtualization &&
                groupCount === 0) {
                virtualHeight = totalItemRowHeight - (2 * pageSizeRowHeight);
            }
            else if (this.isDefaultGrouping()) {
                virtualHeight = visibleGroupedRowsCount * baseRowHeight;
            }
            else {
                virtualHeight = totalItemRowHeight;
            }
        }
        else if (groupCount) {
            virtualHeight = visibleGroupedRowsCount * this.parent.getRowHeight();
        }
        else if (enableVirtualization) {
            virtualHeight = frozenColumns > 0 && enableColumnVirtualization ?
                totalItemRowHeight - pageSizeRowHeight : totalItemRowHeight;
        }
        this.virtualEle.setVirtualHeight(virtualHeight, width);
        if (enableColumnVirtualization) {
            this.header.virtualEle.setVirtualHeight(1, width);
        }
    }

    /**
     * Calculates the virtual page number based on the scroll position from the top.
     * Uses binary search-like logic through offset keys to determine which page corresponds
     * to the current scroll position, accounting for grouped and non-grouped scenarios.
     *
     * @private
     * @param {number} sTop - The scroll top position in pixels
     * @param {VirtualInfo} info - Virtual information object to be updated with block index
     * @returns {number} The calculated page number (1-indexed, clamped between 1 and maxPage)
     */
    private getPageFromTop(sTop: number, info: VirtualInfo): number {
        // Determine total blocks based on grouping state
        const isGroupedAdaptive: boolean = isGroupAdaptive(this.parent);
        const totalBlocks: number = isGroupedAdaptive
            ? this.getGroupedTotalBlocks()
            : this.getTotalBlocks();

        // Early return for edge cases
        if (!this.offsetKeys || this.offsetKeys.length === 0 || sTop < 0) {
            info.block = 0;
            return 1;
        }

        // Find matching offset block
        let calculatedPage: number = 0;
        const matchFound: boolean = this.offsetKeys.some((offsetKey: string): boolean => {
            // Parse offset key to get block index
            const blockIndex: number = Number(offsetKey);
            const blockOffset: number = this.offsets[parseInt(offsetKey, 10)];

            // Determine if scroll position matches this block
            const isWithinBlock: boolean = sTop <= blockOffset;
            const isLastBlockExceeded: boolean = blockIndex === totalBlocks && sTop > blockOffset;
            const isMatchingBlock: boolean = isWithinBlock || isLastBlockExceeded;

            if (isMatchingBlock) {
                // Set block type: even blocks = 1, odd blocks = 0
                info.block = blockIndex % 2 === 0 ? 1 : 0;

                // Calculate and clamp page number to valid range [1, maxPage]
                const rawPage: number = this.vHelper.getPage(blockIndex);
                calculatedPage = Math.max(1, Math.min(rawPage, this.maxPage));
            }

            return isMatchingBlock;
        });

        // Fallback: if no match found, return first page
        return matchFound ? calculatedPage : 1;
    }

    /**
     * Calculates the Y-axis translation value for virtual scrolling based on scroll position and viewport height.
     * This method determines the optimal vertical offset to render content blocks efficiently during scrolling.
     *
     * @protected
     * @param {number} sTop - The scroll top position in pixels
     * @param {number} cHeight - The content/viewport height in pixels
     * @param {VirtualInfo} [info] - Optional virtual information object containing page and block indexes
     * @param {boolean} [isOnenter] - Optional flag indicating if called from onEntered sentinel callback
     * @returns {number} The calculated Y-axis translation value in pixels for virtual content positioning
     */
    protected getTranslateY(sTop: number, cHeight: number, info?: VirtualInfo, isOnenter?: boolean): number {
        if (info === undefined) {
            info = { page: this.getPageFromTop(sTop + cHeight, {}) };
            info.blockIndexes = this.vHelper.getBlockIndexes(info.page);
        }
        const block: number = (info.blockIndexes[0] || 1) - 1;
        const translate: number = this.getOffset(block);
        const endTranslate: number = this.getOffset(info.blockIndexes[info.blockIndexes.length - 1]);
        if (isOnenter) {
            info = this.prevInfo;
        }
        let result: number = translate > sTop ?
            this.getOffset(block - 1) : endTranslate < (sTop + cHeight) ? this.getOffset(block + 1) : translate;
        const blockHeight: number = this.offsets[info.blockIndexes[info.blockIndexes.length - 1]] -
            this.tmpOffsets[info.blockIndexes[0]];
        if (result + blockHeight > this.offsets[isGroupAdaptive(this.parent) ? this.getGroupedTotalBlocks() : this.getTotalBlocks()]
            && this.parent.options.groupCount === 0) {
            result -= (result + blockHeight) - this.offsets[this.getTotalBlocks()];
        }
        return result;
    }

    public getOffset(block: number): number {
        return Math.min(this.offsets[parseInt(block.toString(), 10)] | 0, this.offsets[this.maxBlock] | 0);
    }

    private onEntered(): Function {
        return (current: SentinelType, isLightScroll: boolean, direction: string,
                e: Offsets, isWheel: boolean, check: boolean) => {
            // eslint-disable-next-line @typescript-eslint/no-this-alias
            const _this: VirtualContentRenderer = this;
            this.observer.options.isWheelScroll = isWheel;
            if (Browser.isIE && !isWheel && check && !this.preventEvent) {
                //ToDo//this.parent.showSpinner();
            }
            // --- SECTION 1: Extract and cache parent options ---
            const { enableRtl, enableColumnVirtualization, frozenColumns,
                enableVirtualMaskRow, pageSize, rowHeight,
                overscanCount, totalItemCount, allowGrouping,
                groupCount, height
            } = this.parent.options;

            // --- SECTION 2: Determine scroll axis and calculate base offsets ---
            const isHorizontalScroll: boolean = current.axis === 'X';
            const previousScrollTop: number = this.prevInfo.offsets ? this.prevInfo.offsets.top : null;
            const contentHeight: number = this.content.getBoundingClientRect().height;
            // Calculate horizontal offset based on axis and column indexes
            const columnIndexForOffset: number = isHorizontalScroll
                ? this.vHelper.getColumnIndexes()[0] - 1
                : this.prevInfo.columnIndexes[0] - 1;
            let horizontalTranslate: number = this.getColumnOffset(columnIndexForOffset);
            // Apply RTL adjustment if needed
            if (enableColumnVirtualization && enableRtl) {
                horizontalTranslate = -1 * horizontalTranslate;
            }
            // Calculate vertical offset using translateY method
            const usesPreviousInfo: boolean = isHorizontalScroll && previousScrollTop === e.top;
            let verticalTranslate: number = this.getTranslateY(
                e.top,
                contentHeight,
                usesPreviousInfo ? this.prevInfo : undefined,
                true
            );
            // Adjust Y for horizontal scroll with current info
            if (this.currentInfo && this.currentInfo.startIndex && isHorizontalScroll) {
                const currentEndIndex: number = isNullOrUndefined(this.currentInfo.endIndex)
                    ? 0
                    : this.currentInfo.endIndex;
                verticalTranslate = (currentEndIndex - pageSize) * this.parent.getRowHeight();
            }

            // --- SECTION 3: Calculate movable translate values ---
            // Determine X translation for movable content
            const hasColumnOffset: boolean = Boolean(this.vHelper.cOffsets[this.startColIndex - 1]);
            this.movableTranslateX = hasColumnOffset
                ? this.vHelper.cOffsets[this.startColIndex - 1]
                : horizontalTranslate;
            // Apply RTL adjustment to movable X
            if (enableColumnVirtualization && enableRtl) {
                this.movableTranslateX = -1 * this.movableTranslateX;
            }
            // Override with frozen column offset if applicable
            const hasFrozenColumnOffset: boolean = enableColumnVirtualization &&
                frozenColumns > 0 &&
                Boolean(this.vHelper.mOffsets[this.startColIndex - 1]);
            if (hasFrozenColumnOffset) {
                this.movableTranslateX = this.vHelper.mOffsets[this.startColIndex - 1];
            }
            // Calculate Y translation for movable content
            const effectiveRowHeight: number = rowHeight > 0
                ? rowHeight
                : this.parent.getRowHeight();
            this.movableTranslateY = pageSize * effectiveRowHeight;

            // --- SECTION 4: Handle non-virtual-mask-row scenarios ---
            if (!enableVirtualMaskRow) {
                // Only process when overscan is disabled
                if (overscanCount === 0) {
                    // Handle end-of-data downward scroll
                    const isAtEndScrollingDown: boolean =
                        this.currentInfo.endIndex === totalItemCount &&
                        direction === 'down';
                    if (isAtEndScrollingDown) {
                        // Recalculate verticalTranslate based on start index
                        verticalTranslate = this.currentInfo.startIndex * effectiveRowHeight;
                        // Cache options to avoid repeated property access and improve readability
                        const { requestType, isAdd, newRowPosition, editMode } = this.parent.options;
                        const isBottomSaveAdd: boolean =
                            (requestType === 'Save' || isAdd) &&
                            newRowPosition === 'Bottom' &&
                            editMode === 'Normal';
                        // Prevent scroll jump for bottom add
                        if (isBottomSaveAdd) {
                            verticalTranslate = this.preventScrollJump(verticalTranslate);
                        }
                        // Apply table adjustment with bounds checking
                        const maxVerticalOffset: number = this.offsets[this.maxBlock];
                        const finalYTranslate: number = isBottomSaveAdd
                            ? verticalTranslate
                            : Math.min(verticalTranslate, maxVerticalOffset);
                        this.virtualEle.adjustTable(this.movableTranslateX, finalYTranslate);
                    }
                    // Handle upward scroll or vertical axis with no horizontal prevention
                    else {
                        const shouldAdjustTable: boolean =
                            (!isHorizontalScroll || direction === 'up') &&
                            this.observer.PreventAdjustTable !== 'horizontal';

                        if (shouldAdjustTable) {
                            const maxVerticalOffset: number = this.offsets[this.maxBlock];
                            this.virtualEle.adjustTable(
                                this.movableTranslateX,
                                Math.min(verticalTranslate, maxVerticalOffset)
                            );
                        }
                    }
                }
            }
            // --- SECTION 5: Handle virtual-mask-row scenarios ---
            else {
                // Determine total item count based on grouping
                let isNotAtEnd: boolean = false;
                const isDefaultGrouping: boolean = this.isDefaultGrouping();
                const totalCount: number = isDefaultGrouping
                    ? this.getVisibleGroupedRowCount()
                    : totalItemCount;
                // Check if verticalTranslate is within bounds and not at end
                const isWithinBounds: boolean = this.offsets[this.maxBlock] >= verticalTranslate;
                
                //Overscan with normal grouping scenario
                if (this.parent.options.overscanCount > 0 && isDefaultGrouping) {
                    const calculatedEndIndex: number = this.currentInfo.endIndex + this.parent.options.overscanCount;
                    isNotAtEnd = calculatedEndIndex < this.getVisibleGroupedRowCount();
                }
                else {
                    isNotAtEnd = this.currentInfo.endIndex !== totalCount;
                }
                if (isWithinBounds && isNotAtEnd) {
                    const isHorizontalDirection: boolean = direction === 'right' || direction === 'left';
                    if (isHorizontalDirection) {
                        const previousColumnIndexes: number[] = this.parent.getColumnIndexesInView();
                        const currentViewInfo: VirtualInfo = this.getInfoFromView(direction, current, e);
                        // Check if column indexes have changed
                        const columnIndexesChanged: boolean =
                            JSON.stringify(previousColumnIndexes) !== JSON.stringify(currentViewInfo.columnIndexes);
                        if (columnIndexesChanged) {
                            // Adjust table for horizontal scroll with mask row offset
                            const maskRowYOffset: number = this.translateMaskY + this.movableTranslateY;
                            this.virtualEle.adjustTable(this.movableTranslateX, maskRowYOffset);
                        }
                    }
                    else {
                        // Handle vertical scrolling (up/down)
                        const blockDifference: number = enableColumnVirtualization
                            ? (enableVirtualMaskRow ? 4 : 2)
                            : 0;
                        setTimeout(function (): void {
                            let downwardScrollY: number = verticalTranslate;
                            const gridRowHeight: number = _this.parent.options.rowHeight !== 0 ? _this.parent.options.rowHeight
                                : _this.parent.getRowHeight(true);
                            let rowsInDOM: number = 0;
                            // Handle overscan-disabled scenario
                            if (enableVirtualMaskRow && overscanCount === 0) {
                                // Calculate Y for downward scroll based on scroll position
                                downwardScrollY = _this.content.scrollTop - (2 * pageSize * gridRowHeight);
                                // Get actual row count in DOM
                                rowsInDOM = allowGrouping && groupCount > 0
                                    ? _this.parent.getContent().querySelectorAll('tr').length
                                    : _this.parent.element.querySelectorAll('.e-row').length;
                            }
                            // Handle overscan-enabled scenario
                            else {
                                const actualStartIndex: number = _this.currentInfo.startIndex;
                                const viewportFirstDatarow: Element = _this.content.querySelectorAll('.e-row:not(.e-masked-row)')[0];
                                const viewportStartIndex: number = viewportFirstDatarow
                                    ? parseInt(viewportFirstDatarow.getAttribute('aria-rowindex') as string, 10) : 0;
                                let calculatedOverscan: number = actualStartIndex - viewportStartIndex;
                                // Adjust overscan for non-percentage heights
                                if (height !== '100%') {
                                    const isInvalidOverscan: boolean = isNaN(calculatedOverscan) || calculatedOverscan < 0;
                                    calculatedOverscan = isInvalidOverscan
                                        ? (2 * calculatedOverscan)
                                        : calculatedOverscan;
                                }
                                // Calculate Y for downward scroll with overscan
                                downwardScrollY = _this.content.scrollTop - (2 * (pageSize + calculatedOverscan) * gridRowHeight);
                                rowsInDOM = _this.parent.element.querySelectorAll('.e-row').length;
                            }

                            // Prevent scroll jump for both directions
                            //sets downwardScrollY when we scroll downwards.
                            downwardScrollY = _this.preventScrollJump(downwardScrollY, rowsInDOM);
                            //sets verticalTranslate when we scroll upwards.
                            verticalTranslate = _this.preventScrollJump(verticalTranslate, rowsInDOM);

                            //If isLightScroll is true, then we update _this.translateMaskY which is the translate set in the onDataReady based on the previous data rendering.
                            // Determine minimum Y value based on scroll type and direction
                            const baseYForTranslate: number = isLightScroll
                                ? _this.translateMaskY
                                : (direction === 'down' ? downwardScrollY : verticalTranslate);

                            // Calculate final translate Y with bounds checking
                            let finalTranslateY: number = Math.min(
                                baseYForTranslate,
                                _this.offsets[_this.maxBlock - blockDifference]
                            );
                            // Handle special case: upward scroll to top
                            const isUpwardScrollToTop: boolean = direction === 'up' &&
                                finalTranslateY === 0 &&
                                verticalTranslate === 0;
                            const hasNoStartIndex: boolean =
                                isNullOrUndefined(_this.prevInfo.startIndex) &&
                                isNullOrUndefined(_this.currentInfo.startIndex);
                            const hasBothZeroStartIndex: boolean =
                                _this.prevInfo.startIndex === 0 &&
                                _this.currentInfo.startIndex === 0;
                            const needsTopAdjustment: boolean =
                                isUpwardScrollToTop &&
                                (hasNoStartIndex || hasBothZeroStartIndex || _this.parent.isMacOS());
                            if (needsTopAdjustment) {
                                const startIndex: number = _this.currentInfo.startIndex === 0 ||
                                    isNullOrUndefined(_this.currentInfo.startIndex) ? 0 : _this.currentInfo.startIndex;
                                finalTranslateY = startIndex - (pageSize * gridRowHeight);
                            }
                            // Apply final table adjustment
                            _this.virtualEle.adjustTable(horizontalTranslate, finalTranslateY);
                        }, 0);
                    }
                }
            }
            // --- SECTION 6  : Handle horizontal axis refresh ---
            if (isHorizontalScroll) {
                this.setColVTableWidthAndTranslate({ refresh: true, axis: 'X', direction: direction });
            }
        };
    }

    private preventScrollJump(translate: number, rowsInDOM?: number): number {
        rowsInDOM = isNullOrUndefined(rowsInDOM) ? this.parent.element.querySelectorAll('.e-row').length : rowsInDOM;
        const { rowHeight, groupCount, allowGrouping,
            totalItemCount, enableVirtualMaskRow } = this.parent.options;
        const gridRowHeight: number = rowHeight || this.parent.getRowHeight(true);
        const totalCount: number = allowGrouping && groupCount > 0
            && enableVirtualMaskRow ? this.getVisibleGroupedRowCount() : totalItemCount;
        const virtualTableHeight: number = totalCount * gridRowHeight;
        //The below condition will return true when the scrollHeight for the grid content elements increase above the virtual table height.
        //If it increases, then we will face the scroll jump issue. To avoid the scroll jump issue, we are preventing the scrollheight
        //to be less than or equal to the virtual table height by using below calculation.
        if (translate + (rowsInDOM * gridRowHeight) > virtualTableHeight) {
            translate = virtualTableHeight - (rowsInDOM * gridRowHeight);
        }
        return translate;
    }

    /**
     * Binds the scroll event observer to monitor and handle virtual scrolling.
     * This method sets up the intersection observer to track scroll position changes
     * and triggers appropriate rendering callbacks. Also restores persisted scroll
     * position if persistence is enabled.
     *
     * @public
     * @returns {void}
     */
    public bindScrollEvent: Function = () => {
        // --- SECTION 1: Disconnect existing observer to prevent memory leaks ---
        if (!isNullOrUndefined(this.observer)) {
            this.observer.disconnect();
        }
        const gridObject: SfGrid = this.parent;
        const { enableVirtualization, enableColumnVirtualization,
            enablePersistence
        } = gridObject.options;
        const scrollCallback: (scrollArgs: ScrollArg) => void = (scrollArgs: ScrollArg): void => {
            // Cache parent options to avoid repeated property access
            if (enableVirtualization || enableColumnVirtualization) {
                this.scrollListener(scrollArgs);
            }
        };
        // Attach observer with scroll callback and sentinel enter callback
        this.observer.observe(scrollCallback, this.onEntered());
        // --- SECTION 2: Handle persisted scroll position restoration ---
        if (enablePersistence && gridObject.scrollPosition) {
            this.content.scrollTop = gridObject.scrollPosition.top;
            const restoredScrollArgs: ScrollArg = {
                scrollDirection: 'down',
                sentinel: this.observer.sentinelInfo.down,
                offset: gridObject.scrollPosition,
                focusElement: gridObject.element,
                isWheelScroll: this.observer.options.isWheelScroll
            };
            this.scrollListener(restoredScrollArgs);
            if (enableColumnVirtualization) {
                this.content.scrollLeft = gridObject.scrollPosition.left;
            }
        }
    };


    public getBlockSize(): number {
        return this.parent.options.pageSize >> 1;
    }

    public getBlockHeight(): number {
        return this.getBlockSize() * this.parent.getRowHeight();
    }

    public getGroupedTotalBlocks(): number {
        if (this.parent.options.enableLazyLoading && this.parent.options.enableVirtualization) {
            return this.getTotalBlocks();
        }

        const visibleGroupedRowCount: number = this.getVisibleGroupedRowCount();
        // Get the block size (number of rows per block)
        const blockSize: number = this.getBlockSize();
        // Guard against division by zero
        if (blockSize === 0) {
            return 1;
        }
        const calculatedBlocks: number = visibleGroupedRowCount / blockSize;
        const isLessThanOneBlock: boolean = calculatedBlocks < 1;
        const totalBlocks: number = isLessThanOneBlock ? 1 : Math.floor(calculatedBlocks);
        return totalBlocks;
    }

    public getVisibleGroupedRowCount(): number {
        const placeHolderHeight: string = this.virtualEle.placeholder.style.height;
        const virtualTrackHeight: string = placeHolderHeight.substring(0, placeHolderHeight.indexOf('p'));
        const visibleRowCount: number = Number(virtualTrackHeight) / this.parent.getRowHeight();
        return Math.round(visibleRowCount);
    }

    public getTotalBlocks(): number {
        return Math.ceil(!isNullOrUndefined(this.count) ? this.count / this.getBlockSize() : 0);
    }

    public getColumnOffset(block: number): number {
        const { mOffsets, cOffsets } = this.vHelper;
        if (isNullOrUndefined(block)) {
            return 0;
        }
        if (this.parent.options.frozenColumns > 0) {
            return block > Object.keys(mOffsets).length - 1
                ? cOffsets[parseInt(block.toString(), 10)] | 0
                : mOffsets[parseInt(block.toString(), 10)] | 0;
        }
        return cOffsets[parseInt(block.toString(), 10)] | 0;
    }

    private resetScrollPosition(action: string): void {
        const lowercaseAction: string = !isNullOrUndefined(action) ? action.toLowerCase() : action;
        const isActionInList: boolean = this.actions.some((value: string) => value === action);
        const hasInitialGrouping: boolean = !isNullOrUndefined(this.parent.options.initGroupingField) &&
            this.parent.options.initGroupingField.length > 0;
        const isGroupingRelatedAction: boolean = !isNullOrUndefined(lowercaseAction) &&
            (lowercaseAction === 'sorting' || lowercaseAction === 'save' || lowercaseAction === 'delete');
        const requiresGroupingReset: boolean = hasInitialGrouping && isGroupingRelatedAction;

        // Determine if scroll should be reset
        const shouldResetScroll: boolean = isActionInList || requiresGroupingReset;
        if (shouldResetScroll) {
            const contentElement: Element = this.content;
            this.preventEvent = contentElement.scrollTop !== 0;
            contentElement.scrollTop = 0;
            this.observer.options.previousTop = 0;
            this.currentInfo.startIndex = this.currentInfo.startIndex !== 0 ? 0 : this.currentInfo.startIndex;
            this.currentInfo.endIndex = this.currentInfo.startIndex === 0 ?
                this.currentInfo.startIndex + this.parent.options.pageSize : this.currentInfo.endIndex;
        }
    }

    /**
     * Refresh offsets for some functionality.
     *
     * @returns {void}
     */
    public refreshOffsets(): void {
        //const gObj: SfGrid = this.parent;
        let row: number = 0;
        const blockSize: number = this.getBlockSize();
        const totalBlocks: number = isGroupAdaptive(this.parent) ? this.getGroupedTotalBlocks() : this.getTotalBlocks();
        this.maxBlock = totalBlocks % 2 === 0 ? totalBlocks - 2 : totalBlocks - 1;
        this.offsets = {};
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const vcRows: any = [];
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const cache: any = {};
        //Row offset update
        const gridRowHeight: number = this.parent.getRowHeight();

        // --- SECTION 2: Calculate row offsets for each virtual block ---
        const blocks: number[] = Array(totalBlocks).fill(null).map(() => ++row);
        for (let i: number = 0; i < blocks.length; i++) {
            const tmp: number = (cache[blocks[parseInt(i.toString(), 10)]] || []).length;
            const isGroupAdaptiveGrid: boolean = isGroupAdaptive(this.parent);
            const rem: number = !isGroupAdaptiveGrid ? this.count % blockSize : (vcRows.length % blockSize);
            const size: number = !isGroupAdaptiveGrid && blocks[parseInt(i.toString(), 10)] in cache ?
                tmp * gridRowHeight : rem && blocks[parseInt(i.toString(), 10)] === totalBlocks ? rem * gridRowHeight :
                    this.getBlockHeight();
            this.offsets[parseInt(blocks[parseInt(i.toString(), 10)].toString(), 10)] =
                (this.offsets[parseInt((blocks[parseInt(i.toString(), 10)] - 1).toString(), 10)] | 0) + size;
            this.tmpOffsets[parseInt(blocks[parseInt(i.toString(), 10)].toString(), 10)] =
                this.offsets[parseInt((blocks[parseInt(i.toString(), 10)] - 1).toString(), 10)] | 0;
        }
        this.offsetKeys = Object.keys(this.offsets);
        //Column offset update
        if (this.parent.options.enableColumnVirtualization) {
            this.vHelper.refreshColOffsets();
        }
    }

    public updateTransform(x: number, y: number, isOverscan: boolean, isBottomAdd: boolean): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: VirtualContentRenderer = this;
        // --- SECTION 1: Cache grid options to avoid repeated property access ---
        const { rowHeight, enableVirtualMaskRow, enableLazyLoading,
            requestType, pageSize, overscanCount, enableVirtualization,
            totalItemCount, editMode, newRowPosition
        } = _this.parent.options;
        const gridRowHeight: number = rowHeight || this.parent.getRowHeight();

        // --- SECTION 2: Handle regular virtual scrolling (non-bottom-add scenario) ---
        if (!isBottomAdd) {
            let verticalTranslation: number = 0;
            // Check if virtual mask row mode is active
            const hasVirtualMaskOrLazyLoad: boolean =
                (enableVirtualMaskRow && enableVirtualization) || enableLazyLoading;
            const isNotGroupExpandCollapse: boolean = requestType !== 'GroupExpandCollapse';
            const shouldCalculateMaskTranslation: boolean = hasVirtualMaskOrLazyLoad && isNotGroupExpandCollapse;
            if (shouldCalculateMaskTranslation) {
                // Calculate base start index for current viewport
                let startIndex: number = _this.currentInfo.endIndex - pageSize;
                startIndex = startIndex > 0 ? startIndex : 0;
                // Calculate base vertical translation
                verticalTranslation = startIndex * gridRowHeight;
                // Guard against NaN values
                verticalTranslation = isNaN(verticalTranslation) ? 0 : verticalTranslation;
                // Calculate overscan offset if applicable
                let overscanRowCount: number = 0;
                if (overscanCount > 0) {
                    const actualStartIndex: number = _this.currentInfo.startIndex || 0;
                    const viewportFirstDatarow: Element = _this.content.querySelectorAll('.e-row:not(.e-masked-row)')[0];
                    const viewportStartIndex: number = viewportFirstDatarow ? parseInt(viewportFirstDatarow.getAttribute('aria-rowindex'), 10) - 1 : 0;
                    overscanRowCount = actualStartIndex - viewportStartIndex;
                }
                // Adjust translation to account for page size and overscan
                const totalRowOffset: number = pageSize + overscanRowCount;
                verticalTranslation = verticalTranslation - (totalRowOffset * gridRowHeight);
            }
            setTimeout(() => {
                _this.translateMaskX = x;
                if (isOverscan) {
                    _this.translateMaskY = _this.preventScrollJump(verticalTranslation);
                }
                _this.virtualEle.adjustTable(x, _this.translateMaskY);
            }, 500);
            return;
        }

        // --- SECTION 3: Handle bottom row addition scenario ---
        // Calculate effective page size including overscan
        let effectivePageSize: number = overscanCount ? pageSize + (overscanCount * 2) : pageSize;
        // Clamp page size to total item count
        effectivePageSize = totalItemCount < effectivePageSize ? totalItemCount : effectivePageSize;
        // Update current info to reflect bottom position
        _this.currentInfo.endIndex = totalItemCount;
        const startIndex: number = _this.currentInfo.startIndex = _this.currentInfo.endIndex - effectivePageSize;
        let verticalTranslation: number = startIndex * gridRowHeight;
        if (enableVirtualMaskRow) {
            verticalTranslation = verticalTranslation - (pageSize * gridRowHeight);
            _this.preventScrollJump(verticalTranslation);
        }
        const finalTranslation: number = startIndex > 0 ? _this.preventScrollJump(verticalTranslation) : verticalTranslation;
        this.virtualEle.wrapper.style.transform = `translate(0px, ${finalTranslation}px)`;
        // Handle special case: Normal edit mode with bottom row save
        const isNormalEditBottomSave: boolean = editMode === 'Normal' && requestType === 'Save' && newRowPosition === 'Bottom';
        if (isNormalEditBottomSave) {
            this.parent.getContent().scrollTop += gridRowHeight;
        }
    }

    public refreshColumnIndexes(): void {
        this.vHelper.refreshColOffsets();
        const colIndexes: number[] = this.vHelper.getColumnIndexes();
        this.parent.setColumnIndexesInView(colIndexes);
        this.parent.dotNetRef.invokeMethodAsync('SetColumnIndexes', colIndexes[0], colIndexes[colIndexes.length - 1]);
    }

    public refreshVirtualElement(): void {
        this.vHelper.refreshColOffsets();
        this.setVirtualHeight();
    }
    private isDefaultGrouping(): boolean {
        const { allowGrouping, enableLazyLoading, groupCount } = this.parent.options;
        return allowGrouping && !enableLazyLoading && groupCount > 0;
    }
}
/**
 * @hidden
 */
export class VirtualHeaderRenderer {
    public virtualEle: VirtualElementHandler;
    private vHelper: VirtualHelper;
    private parent: SfGrid;
    private headerPanel: Element;

    constructor(parent: SfGrid) {
        this.parent = parent;
        this.vHelper = new VirtualHelper(this.parent);
        this.virtualEle = new VirtualElementHandler(this.parent);
        this.headerPanel = this.parent.element.querySelector('.e-gridheader');
    }

    /**
     * Get the header content div element of grid
     *
     * @returns {void}
     */
    public getPanel(): Element {
        return this.headerPanel;
    }

    /**
     * Get the header table element of grid
     *
     * @returns {void}
     */
    public getTable(): Element {
        return this.headerPanel.querySelector('.e-table');
    }

    public renderTable(): void {
        this.vHelper.refreshColOffsets();
        this.parent.setColumnIndexesInView(this.vHelper.getColumnIndexes(<HTMLElement>this.getPanel().querySelector('.e-headercontent')));
        this.virtualEle.table = <HTMLElement>this.getTable();
        this.virtualEle.content = <HTMLElement>this.getPanel().querySelector('.e-headercontent');
        this.virtualEle.content.style.position = 'relative';
        this.virtualEle.renderWrapper();
        this.virtualEle.renderPlaceHolder();
    }

}
/**
 * @hidden
 */
export class VirtualElementHandler {
    public wrapper: HTMLElement;
    public movableHeaderWrapper: HTMLElement;
    public movableContentWrapper: HTMLElement;
    public placeholder: HTMLElement;
    public content: HTMLElement;
    public table: HTMLElement;
    public parent: SfGrid;
    public filterTranslateX: number;

    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    public renderWrapper(height?: number): void {
        this.wrapper = this.content.querySelector('.e-virtualtable');
        this.wrapper.setAttribute('styles', `min-height:${formatUnit(height)}`);
        const gridContent: HTMLElement = this.content.querySelector('.e-gridcontent') as HTMLElement;
        if (!isNullOrUndefined(gridContent)) {
            this.movableHeaderWrapper = this.content.querySelector('.e-virtualtable');
            this.movableContentWrapper = this.content.querySelector('.e-virtualtable');
        }
    }

    public renderPlaceHolder(): void {
        const contentLastChild: HTMLElement = this.content.lastElementChild as HTMLElement;
        this.placeholder = this.content && contentLastChild instanceof HTMLElement &&
            contentLastChild.classList.contains('e-virtualtrack') ?
            contentLastChild as HTMLElement : (this.content ? this.content.querySelector('.e-virtualtrack') as HTMLElement | null : null);
    }


    public adjustTable(xValue: number, yValue: number, direction?: string): void {
        const { enableVirtualization, enableColumnVirtualization,
            frozenColumns
        } = this.parent.options;
        this.filterTranslateX = xValue;
        if (enableColumnVirtualization && !enableVirtualization && yValue > 0) {
            yValue = 0;
        }
        if (isNullOrUndefined(this.content)) {
            return;
        }
        const frozenCellSelector: NodeListOf<Element> = this.content.querySelectorAll('.e-leftfreeze,.e-rightfreeze,.e-fixedfreeze');
        const cells: HTMLElement[] = [].slice.call(frozenCellSelector);
        let frzLeftWidth: number = 0;
        let frzRightWidth: number = 0;

        // Calculate frozen column widths if Fixed freeze columns exist
        const headerContent: Element = this.parent.getHeaderContent();
        const hasFixedFreezeColumns: boolean = !isNullOrUndefined(headerContent) ? headerContent.querySelectorAll('.e-fixedfreeze').length > 0 :
            false;
        if (hasFixedFreezeColumns) {
            frzLeftWidth = this.parent.leftrightColumnWidth('left');
            frzRightWidth = this.parent.leftrightColumnWidth('right');
        }
        if (cells.length > 0) {
            for (let i: number = 0; i < cells.length; i++) {
                const cell: HTMLElement = cells[parseInt(i.toString(), 10)];
                let col: Column = null;
                // Determine column object based on cell type
                const isRowCell: boolean = cell.classList.contains('e-rowcell');
                const isHeaderCell: boolean = cell.classList.contains('e-headercell');
                const isFilterBarCell: boolean = cell.classList.contains('e-filterbarcell');
                if (isRowCell) {
                    // Handle row cell - get column by UID or index
                    const mappingUidElement: Element = cell.querySelector('[e-mappinguid]');
                    const hasAriaColIndex: boolean = !isNullOrUndefined(parseInt(cell.getAttribute('aria-colindex'), 10) - 1);
                    const hasMappingUid: boolean = !isNullOrUndefined(mappingUidElement);
                    if (hasAriaColIndex && hasMappingUid) {
                        const uid: string = mappingUidElement.getAttribute('e-mappinguid');
                        col = this.parent.getColumnByUid(uid);
                    }
                    else if (hasAriaColIndex) {
                        const idx: number = parseInt(cell.getAttribute('aria-colindex'), 10) - 1;
                        col = this.parent.getColumnByIndex(parseInt(idx.toString(), 10), true);
                    }
                }
                else {
                    if (isHeaderCell || isFilterBarCell) {
                        let uid: string = null;
                        if (isFilterBarCell) {
                            uid = cell.getAttribute('e-mappinguid');
                        } else if (isHeaderCell) {
                            const mappingUidElement: Element = cell.querySelector('[e-mappinguid]');
                            if (!isNullOrUndefined(mappingUidElement)) {
                                uid = mappingUidElement.getAttribute('e-mappinguid');
                            }
                        }
                        if (!isNullOrUndefined(uid)) {
                            col = this.parent.getColumnByUid(uid);
                        }
                    }
                }
                if (!isNullOrUndefined(col)) {
                    const freezeType: string = col.freeze;
                    if (freezeType === 'Left') {
                        cell.style.left = (col.translateLeftRightValue - xValue) + 'px';
                    }
                    else if (freezeType === 'Right') {
                        cell.style.right = (col.translateLeftRightValue + xValue) + 'px';
                    } else if (freezeType === 'Fixed') {
                        cell.style.left = (frzLeftWidth - xValue) + 'px';
                        cell.style.right = (frzRightWidth + xValue) + 'px';
                    }
                }
            }
        }
        const trackWidth: number = this.placeholder.offsetWidth;
        const contentOffsetWidth: number = (this.parent.getContentTable() as HTMLElement).offsetWidth;
        const wouldExceedTrackWidth: boolean = (contentOffsetWidth + xValue) > trackWidth;
        if (direction === 'right' && wouldExceedTrackWidth && !frozenColumns) {
            //The above condition statisfies only if we set this xValue to the virtualtable, then the content scrollWidth will exceed
            //the actual trackWidth of the content element. So horizontal scroll jump issue will occur. To avoid this jump issue,
            //we are recalculating the xValue in below line.
            xValue = trackWidth - contentOffsetWidth;
        }
        yValue = enableVirtualization ? yValue : 0;
        this.wrapper.style.transform = 'translate(' + xValue + 'px, ' + yValue + 'px)';
    }

    public extractTranslateY(transformStyle: string): string {
        const match: RegExpMatchArray | null = transformStyle.match(/translate\([^,]+,([^)]+)\)/);
        if (match && match[1]) {
            return match[1].trim();
        }
        return '0';
    }

    public setWrapperWidth(width: string, full?: boolean): void {
        this.wrapper.style.width = width ? `${width}px` : full ? '100%' : '';
    }

    public setVirtualHeight(height?: number, width?: string): void {
        if (this.parent.options.enableVirtualization) {
            this.placeholder.style.height = `${height}px`;
        }
        if (!isNullOrUndefined(this.content) && this.content.classList.contains('e-content') && !this.parent.options.enableVirtualization
            && this.parent.isMacSafariBrowser()) {
            this.placeholder.style.height = '1px';
        }
        if (isNullOrUndefined(this.placeholder)) {
            return;
        }
        this.placeholder.style.width = width;
    }

}

/**
 * Content module is used to render grid content
 */
export class VirtualHelper {
    public parent: SfGrid;
    public cOffsets: { [x: number]: number } = {};
    public mOffsets: { [x: number]: number } = {};
    public data: { [x: number]: Object[] } = {};
    public groups: { [x: number]: Object } = {};

    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    public getBlockIndexes(page: number): number[] {
        return [page + (page - 1), page * 2];
    }

    public getPage(block: number): number {
        return block % 2 === 0 ? block / 2 : (block + 1) / 2;
    }

    public getData(): VirtualInfo {
        return {
            page: this.parent.options.currentPage,
            blockIndexes: this.getBlockIndexes(this.parent.options.currentPage),
            direction: 'down',
            columnIndexes: this.parent.getColumnIndexesInView()
        };
    }

    public getColumnIndexes(content: HTMLElement =
    (<HTMLElement>this.parent.getHeaderContent())): number[] {
        if (!isNullOrUndefined(this.parent.getContent())) {
            content = this.parent.getContent();
        }
        let indexes: number[] = [];
        let sLeft: number = this.parent.options.enableColumnVirtualization
            && this.parent.options.enableRtl ? -1 * content.scrollLeft | 0 : content.scrollLeft | 0;
        const keys: string[] = Object.keys(this.cOffsets);
        const cWidth: number = this.parent.options.needClientAction ? (this.parent.options.frozenColumns
            && this.parent.options.enableColumnVirtualization ?
            this.parent.getHeaderContent().offsetWidth : content.getBoundingClientRect().width) :
            (parseInt(this.parent.options.width, 10));
        sLeft = Math.min(this.cOffsets[keys.length - 1] - cWidth, sLeft);
        const calWidth: number = Browser.isDevice ? 2 * cWidth : cWidth / 2;
        let left: number = sLeft + cWidth + (sLeft === 0 ? calWidth : 0);
        keys.some((offset: string) => {
            const iOffset: number = Number(offset); const offsetVal: number = this.cOffsets[`${offset}`];
            const border: boolean = sLeft - calWidth <= offsetVal && left + calWidth >= offsetVal;
            if (border) {
                indexes.push(iOffset);
            }
            return left + calWidth < offsetVal;
        });
        if (!indexes.length && !this.parent.options.frozenColumns) {
            if (keys.length > 0) {
                const firstColOffset: number = this.cOffsets[`${keys[0]}`];
                if (!isNullOrUndefined(firstColOffset)) {
                    left = left + firstColOffset;
                    keys.some((offset: string) => {
                        const iOffset: number = Number(offset); const offsetVal: number = this.cOffsets[`${offset}`];
                        const border: boolean = sLeft - calWidth <= offsetVal && left + calWidth >= offsetVal;
                        if (border) {
                            indexes.push(iOffset);
                        }
                        return left + calWidth < offsetVal;
                    });
                }
            }
        }
        if (indexes.length === 1 && !this.parent.options.frozenColumns) {
            this.parent.virtualContentModule.startColIndex = indexes[0];
            this.parent.virtualContentModule.endColIndex = indexes[0] + 1;
            indexes.push(indexes[0] + 1);
        } else {
            this.parent.virtualContentModule.startColIndex = indexes[0];
            this.parent.virtualContentModule.endColIndex = indexes[indexes.length - 1];
        }
        return indexes;
    }

    public refreshColOffsets(): void {
        let col: number = 0;
        this.cOffsets = {};
        this.mOffsets = {};
        const { groupCount, columns, actualFrozenColumns,
            frozenColumns, enableColumnVirtualization
        } = this.parent.options;
        const gLen: number = groupCount;
        let cols: Column[] = columns;
        if (enableColumnVirtualization) {
            cols = cols.filter((e: Column) => e.visible);
        }
        const cLen: number = cols.length;
        const blocks: number[] = Array(cLen).fill(null).map(() => col++);
        for (let j: number = 0; j < blocks.length; j++) {
            this.cOffsets[blocks[parseInt(j.toString(), 10)]] = (this.cOffsets[blocks[parseInt(j.toString(), 10)] - 1] | 0) +
                (cols[parseInt(j.toString(), 10)].visible ? parseInt(cols[parseInt(j.toString(), 10)].width.toString(), 10) : 0);
        }
        if (frozenColumns) {
            let mCol: number = 0;
            let mCols: Column[];
            if (!actualFrozenColumns) {
                const fCols: Column[] = cols.filter((col: Column) => col.isFrozen && (col.freeze === 'Left' || col.freeze === 'Right'));
                mCols = cols.filter((col: Column) => !fCols.some((fCol: Column) => fCol === col));
            }
            else {
                let fColsLen: number = 0;
                for (let i: number = 0; i < cols.length; i++) {
                    if (!cols[parseInt(i.toString(), 10)].isFrozen && cols[parseInt(i.toString(), 10)].index < frozenColumns) {
                        fColsLen++;
                    }
                    else {
                        break;
                    }
                }
                mCols = cols.slice(fColsLen, cLen);
            }
            const mColLen: number = mCols.length;
            const blocks: number[] = Array(mColLen).fill(null).map(() => mCol++);
            for (let j: number = 0; j < blocks.length; j++) {
                blocks[parseInt(j.toString(), 10)] = blocks[parseInt(j.toString(), 10)] + gLen;
                this.mOffsets[blocks[parseInt(j.toString(), 10)]] = (this.mOffsets[blocks[parseInt(j.toString(), 10)] - 1] | 0) +
                    (mCols[parseInt(j.toString(), 10)].visible ? parseInt(<string>mCols[parseInt(j.toString(), 10)].width, 10) : 0);
            }
        }
    }
}

type ScrollArg = { scrollDirection: string, sentinel: SentinelType, offset: Offsets, focusElement: HTMLElement, isWheelScroll: boolean };

export type SentinelType = {
    check?: (rect: ClientRect, info: SentinelType) => boolean,
    top?: number, entered?: boolean,
    axis?: string;
};

export type SentinelInfo = { up?: SentinelType, down?: SentinelType, right?: SentinelType, left?: SentinelType };

export type Offsets = { top?: number, left?: number };
