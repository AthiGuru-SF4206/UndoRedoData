import { BlazorDotnetObject, isNullOrUndefined, enableBlazorMode, closest } from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { getCellByRowUidAndColIndex, getScrollBarWidth } from './util';
import { BlazorGridElement, IGridOptions, Column, FreezeLineMovingClientOptions, InitModulesResults, FocusEditableCellsArgs } from './interfaces';
import { ColumnWidthService } from './width-controller';
import { parentsUntil } from './util';
/**
 * Blazor grid interop handler
 */
// tslint:disable
const Grid: object = {
    initialize(dataId: string, element: BlazorGridElement, options: IGridOptions,
               dotnetRef: BlazorDotnetObject, focusEditableCellsParams?: FocusEditableCellsArgs | null): InitModulesResults {
        enableBlazorMode();
        new SfGrid(dataId, element, options, dotnetRef);
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (gridInstance.options.showAddNewRow && focusEditableCellsParams != null) {
            const editableCellField: string = focusEditableCellsParams.editableCellField || '';
            const editableIsAdd: boolean = focusEditableCellsParams.isAdd || false;
            const editableFrozenEdit: boolean = focusEditableCellsParams.editableCellFrozenEdit || false;
            this.focusCell(dataId, editableCellField, editableIsAdd, editableFrozenEdit);
        }
        return gridInstance.getInitModulesResults();
    },
    contentReady(dataId: string, options: IGridOptions, action: string, isResetData?: boolean): InitModulesResults {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        let contentReadyResults: InitModulesResults;
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.setOptions(options, instance.options);
            instance.options = options;
            if (!isNullOrUndefined(instance.scrollModule)) {
                instance.scrollModule.setPadding();
            }
            contentReadyResults = instance.contentReady(action, isResetData);
            if (instance.options.isColumnWidthChanged) {
                const widthService = new ColumnWidthService(instance);
                widthService.setWidthToTable();
            }
            if (options.height === '100%') {
                instance.scrollModule.refresh();
            }
        }
        return contentReadyResults;
    },
    refreshPivotRowHeight(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.freezeModule.refreshFreeze({ case: 'textwrap' });
        }

    },
    customFilterDialog(dataId: string, dlgID: string, isExcel: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            let dialogElement: HTMLElement = (document.querySelector('#' + dlgID) as HTMLElement);
            if(gridInstance.options.isRenderedFromTreeGrid && isNullOrUndefined(dialogElement)){
                dialogElement = document.getElementById(dlgID) as HTMLElement;
            }
            dialogElement.style.maxHeight = '100%';
            dialogElement.style.border = '1px';
            dialogElement.style.top = '0px';
            if (isExcel) {
                const contextMenuElements: NodeListOf<HTMLElement> = gridInstance.element.querySelectorAll('.e-sfcontextmenu');
                const filterMenu: HTMLElement = (Array.from(contextMenuElements).find((el: HTMLElement) => el.classList.contains('e-grid-filter-menu')) as HTMLElement);
                if (!isNullOrUndefined(filterMenu)) {
                    const caretElement: HTMLElement = (filterMenu.querySelector('.e-caret') as HTMLElement);
                    if (!isNullOrUndefined(caretElement)) {
                        caretElement.style.paddingRight = '8px';
                    }
                }
            }
        }
    },
    setCustomFilterDialogPadding(element: BlazorGridElement, field: string) {
        const gridInstance = this.sfBlazor.getCompInstance(element);
        let setPadding: HTMLElement = (document.querySelector('#' + field) as HTMLElement);
        if(gridInstance.options.isRenderedFromTreeGrid && isNullOrUndefined(setPadding)){
            setPadding = document.getElementById(field) as HTMLElement;
        }
        (setPadding as HTMLElement).style.padding = '16px';
    },
    searchClear(dataId: string, inputId: string, inputValue: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            let inputElement: HTMLInputElement = gridInstance.element.querySelector('#' + inputId);
            if (gridInstance.options.isRenderedFromTreeGrid && isNullOrUndefined(inputElement))
            {
                inputElement = document.getElementById(inputId) as HTMLInputElement;
            }
            //Note: The variable 'inputValue' is used to update checkbox input element values when the input value is changed using events.
            if (!isNullOrUndefined(inputElement)) {
                inputElement.value = !isNullOrUndefined(inputValue) ? inputValue : '';
                if (!isNullOrUndefined(inputElement.id) && !inputElement.id.includes('CCSearchBox')) {
                    inputElement.focus();
                }
            }
        }
    },
    updateTableWidth(dataId: string, columns: Column[]) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.options.columns = columns;
            if (instance.options.allowResizing && instance.options.isResizedGrid) {
                const widthService = new ColumnWidthService(instance);
                const tablewidth: boolean = columns.some(x => (x.width === '' || x.width === null));
                widthService.setWidthToTable(columns, tablewidth);
            }
        }
    },
    preventResizeAction(dataId: string, isCancel: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.resizeModule.preventResizeAction(isCancel);
        }
    },
    preventFreezeLineMoving(dataId: string, isCancel: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.frozenDragDropModule.preventFreezeLineMoving(isCancel);
        }
    },
    freezeLineMovedActions(dataId: string, freezeLineMovingClientOptions: FreezeLineMovingClientOptions) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.options.actualFrozenColumns = freezeLineMovingClientOptions.actualFrozenColumns;
            instance.options.columns = freezeLineMovingClientOptions.columns;
            instance.options.frozenRightCount = freezeLineMovingClientOptions.frozenRightCount;
            instance.options.frozenLeftCount = freezeLineMovingClientOptions.frozenLeftCount;
            instance.options.frozenLeftColumnsCount = freezeLineMovingClientOptions.frozenLeftColumnsCount;
            instance.options.frozenColumns = freezeLineMovingClientOptions.frozenColumns;
            instance.options.isColumnReordered = freezeLineMovingClientOptions.isColumnReordered;
            instance.freezeLineMovedAction();
            instance.freezeModule.setFrozenHeight();
        }
    },
    frozenHeight(dataId: string, options: IGridOptions) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.freezeModule.setFrozenHeight();
            if (options.allowTextWrap) {
                instance.freezeModule.refreshRowHeight();
                instance.freezeModule.refreshFreeze({
                    'case': 'textwrap'
                });
            }
            if (options.allowResizing) {
                instance.freezeModule.updateResizeHandler();
            }
        }
    },
    updateVirtualColumns(dataId: string, virtualizedColumns: Column[]) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.options.virtualizedColumns = virtualizedColumns;
            if (instance.options.allowResizing && instance.options.frozenColumns === 0 && instance.getContent().querySelector('table').style.width !== '') {
                const widthService: ColumnWidthService = new ColumnWidthService(instance);
                widthService.setWidthToTable(null, false, 'resize');
            }
        }
    },
    updateOptions(dataId: string, options: IGridOptions) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.setOptions(options, instance.options);
        }
    },

    virtualHeight(dataId: string, options: IGridOptions, totalItemCount: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.options = options;
            instance.options.totalItemCount = totalItemCount;
            instance.virtualContentModule.refreshOffsets();
            instance.virtualContentModule.setVirtualHeight();
        }
    },

    lazyGroupExpand(dataId: string, options: IGridOptions) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.setOptions(options, instance.options);
            instance.options = options;
            if (instance.options.enableVirtualization) {
                instance.virtualContentModule.onDataReady();
            }
            if (instance.options.enableInfiniteScrolling) {
                instance.infiniteScrollModule.infiniteOnDataReady();
                instance.infiniteScrollModule.isLazyChildLoad = false;
                instance.infiniteScrollModule.currentRowIndex = 0;
            }
        }
    },

    viewRefresh(dataId: string, columns: Column[]) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const widthService: ColumnWidthService = new ColumnWidthService(gridInstance);
            if(gridInstance.options.enableColumnVirtualization){
                columns = gridInstance.options.virtualizedColumns;
                gridInstance.virtualContentModule.refreshOffsets();
            }
            columns = columns.filter(x => x.visible);
            const tablewidth: boolean = columns.some(x => (x.width === '' || x.width === null || x.width === 'auto'));
            gridInstance.getColumns();
            widthService.setWidthToTable(columns, tablewidth);
            gridInstance.addTableBorderClass();
        }
    },

    virtualDisconnect(dataId: string, options: IGridOptions) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)
            && !isNullOrUndefined(gridInstance.element.blazor__instance.virtualContentModule)) {
            gridInstance.element.blazor__instance.options.enableVirtualization = options.enableVirtualization;
            gridInstance.element.blazor__instance.virtualContentModule.observer.disconnect();
        }
    },

    reorderColumns(dataId: string, fromFName: string | string[], toFName: string) { //NEW
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.reorderModule.reorderColumns(fromFName, toFName);
        }
    },

    reorderColumnByIndex(dataId: string, fromIndex: number, toIndex: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.reorderModule.reorderColumnByIndex(fromIndex, toIndex);
        }
    },

    reorderColumnByTargetIndex(dataId: string, fieldName: string, toIndex: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.reorderModule.reorderColumnByTargetIndex(fieldName, toIndex);
        }
    },
    renderColumnChooser: function (dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        let positions = ['0', '0'];
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            positions = gridInstance.element.blazor__instance.columnChooserModule.renderColumnChooser();
        }
        return positions;
    },

    renderColumnMenu: function (dataId: string, uid: string, isFilter: boolean, key: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            return gridInstance.element.blazor__instance.columnMenuModule.renderColumnMenu(uid, isFilter, key);
        }
        else {
            return { Left: 1, Top: 1 };
        }
    },

    renderAdaptiveMenuItems: function (dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const columnMenuElement: Element = document.getElementsByClassName(`e-${gridInstance.element.id}-column-menu`)[0];
        const element: HTMLElement = !isNullOrUndefined(columnMenuElement) ? columnMenuElement.getElementsByTagName('ul')[0] : null;
        const e: HTMLElement = document.getElementById(gridInstance.element.id + '_responsivetoolbaritems');
        const btnOffset: ClientRect = e.getBoundingClientRect();
        let left: number = btnOffset.left + scrollX;
        let top: number = btnOffset.bottom + scrollY;
        const popupOffset: ClientRect = element.getBoundingClientRect();
        const docElement: HTMLElement = document.documentElement;
        if (btnOffset.bottom + popupOffset.height > docElement.clientHeight) {
            if (top - btnOffset.height - popupOffset.height > docElement.clientTop) {
                top = top - btnOffset.height - popupOffset.height;
            }
        }
        if (btnOffset.left + popupOffset.width > docElement.clientWidth) {
            if (btnOffset.right - popupOffset.width > docElement.clientLeft) {
                left = (left + btnOffset.width) - popupOffset.width;
            }
        }
        left = e.getAttribute('data-index') === '0' ? left - element.getBoundingClientRect().width + popupOffset.width : left - element.getBoundingClientRect().width + btnOffset.width - getScrollBarWidth();
        (columnMenuElement as HTMLElement).style.left = Math.ceil(left + 1) + 'px';
        (columnMenuElement as HTMLElement).style.top = Math.ceil(top + 1) + 'px';
    },

    filterPopupRender: function filterPopupRender(dataId: string, dlgID: string, uid: string, type: string, isColumnMenu: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        let positions = ['0', '0'];
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            positions = gridInstance.element.blazor__instance.filterModule.filterPopupRender(dlgID, uid, type, isColumnMenu);
        }
        return positions;
    },
    clientHeight: function clientHeight(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            return Math.max(gridInstance.element.blazor__instance.content.clientHeight, window.innerHeight || 0);
        }
        return 0;
    },
    clientTransformUpdate: function clientTransformUpdate(dataId: string, xPosition: number, yPosition: number,
                                                          isOverscan: boolean = false, isBottomAdd: boolean = false) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance) &&
            !isNullOrUndefined(gridInstance.element.blazor__instance.virtualContentModule)) {
            gridInstance.element.blazor__instance.virtualContentModule.updateTransform(xPosition, yPosition, isOverscan, isBottomAdd);
        }
    },
    autoFitColumns(dataId: string, columns: Column[], fieldNames: string | string[], isAutoFit: boolean, isColumnResized: boolean = false) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            if (isAutoFit && !isNullOrUndefined(instance.resizeModule)) {
                instance.options.columns = columns;
                instance.resizeModule.autoFitColumns(fieldNames);
            }
            if (isColumnResized) {
                const widthService = new ColumnWidthService(instance);
                widthService.setPersistedWidth(columns[0]);
            }
        }
    },
    autoFit(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.resizeModule.autoFit();
            instance.scrollModule.setPadding();
        }
    },
    refreshColumnIndex(dataId: string, columns: Column[]) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const instance = gridInstance.element.blazor__instance;
            instance.options.columns = columns;
            instance.virtualContentModule.refreshColumnIndexes();
        }
    },
    focus(dataId: string, rowuid: string, celluid: string, action: string, keyCombination?: string,
          headeruid?: string, cellColIndex: number = -1, isSelectionMethodInvoked: boolean = false,
          isLastBatchEditCell: boolean = false) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (isNullOrUndefined(gridInstance)) {
            return;
        }
        let cell: HTMLElement = getCellByRowUidAndColIndex(gridInstance.element, rowuid, cellColIndex);
        cell = cell == null ? gridInstance.element.querySelector('[data-uid="' + celluid + '"]') : cell;
        const headerCell: HTMLElement = gridInstance.element.querySelector('[e-mappinguid="' + headeruid + '"]');
        if (isNullOrUndefined(cell) && isNullOrUndefined(headerCell)) {
            return;
        }
        if (!isNullOrUndefined(headerCell) && !isNullOrUndefined(headerCell.parentElement)
            && !isNullOrUndefined(headerCell.parentElement.parentElement)) {
            headerCell.parentElement.parentElement.focus();
            return;
        }
        const expandCollapseCell: boolean = cell.classList.contains('e-recordplusexpand') || cell.classList.contains('e-recordpluscollapse');
        const isTemplateCell: boolean = isSelectionMethodInvoked || ((keyCombination == null && action == null) ? !cell.classList.contains('e-templatecell'): true);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)
            && !isNullOrUndefined(cell) && isTemplateCell) {
            const instance = gridInstance.element.blazor__instance;
            const { enableVirtualization, enableColumnVirtualization } = instance.options;
            if (!enableVirtualization && !enableColumnVirtualization || (!isNullOrUndefined(action) && (['UpdateRecord', 'ScrollSelect', 'CancelEdit'].indexOf(action) !== -1 ||
                (enableColumnVirtualization && action === 'ScrollSelect'))) ||
                expandCollapseCell) {
                if (!isLastBatchEditCell) {
                    cell.focus();
                }
            } else {
                instance.virtualContentModule.handleCellFocusAndNavigation(cell, action, keyCombination);
            }
        }
    },
    scrollToFocusedCell(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const element: Element | null = document.activeElement;
        if (!element){
            return;// Ensure the focused element exists
        }

        const grid = gridInstance.element;
        const gridRow: HTMLElement = grid.querySelector('.e-row') as HTMLElement;
        const gridContent: HTMLElement = gridInstance.content as HTMLElement;
        const gridRect = grid.getBoundingClientRect();
        const elementRect = element.getBoundingClientRect();

        const getFrozenColumnWidth = (frozenSelector: string): number => {
            return Array.from(gridRow.querySelectorAll(frozenSelector))
                .reduce((totalWidth, col) => totalWidth + (col as HTMLElement).offsetWidth, 0);
        };
        if (!isNullOrUndefined(getFrozenColumnWidth)) {
            return;
        }
        const leftFrozenWidth: number = getFrozenColumnWidth('.e-leftfreeze');
        const rightFrozenWidth: number = getFrozenColumnWidth('.e-rightfreeze');

        if (elementRect.left < gridRect.left + leftFrozenWidth && !element.classList.contains('e-leftfreeze')) {
            gridContent.scrollLeft -= (gridRect.left + leftFrozenWidth - elementRect.left);
        }
        else if (elementRect.right > gridRect.right - rightFrozenWidth && !element.classList.contains('e-rightfreeze')) {
            gridContent.scrollLeft += (elementRect.right - (gridRect.right - rightFrozenWidth)) + 20;
        }

    },
    blurActiveElement(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (isNullOrUndefined(gridInstance) || isNullOrUndefined(gridInstance.element)) {
            return;
        }
        const gridElement = gridInstance.element;
        // If grid has an empty row placeholder, move focus into that cell instead of blurring the active element.
        const emptyCell = gridElement.querySelector('.e-emptyrow td[tabindex]');
        if (!isNullOrUndefined(emptyCell) && gridInstance.options.allowPaging) {
            emptyCell.focus();
            return;
        }
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance) && !isNullOrUndefined(parentsUntil(document.activeElement, 'e-grid'))) {
            (document.activeElement as HTMLElement).blur();
        }
    },
    iterateElements(detailTemplateElements: HTMLElement, childGrid: HTMLElement, isChildGridNull: boolean) {
        for (let i: number = detailTemplateElements.children.length - 1; i >= 0; i--) {
            if (detailTemplateElements.children[parseInt(i.toString(), 10)].classList.contains('sf-grid') && !isChildGridNull) {
                const isPagerNull = isNullOrUndefined(childGrid.querySelector('.e-pagercontainer'));
                const isLastPageIconNull = isNullOrUndefined(childGrid.querySelector('.e-lastpage'));
                if (isPagerNull) {
                    (childGrid.querySelectorAll('.e-rowcell:not(.e-hide)')[childGrid.querySelectorAll('.e-rowcell:not(.e-hide)').length - 1] as HTMLElement).focus();
                } else if (!isPagerNull && !isLastPageIconNull) {
                    (childGrid.querySelector('.e-lastpage') as HTMLElement).focus();
                } else if (!isPagerNull && isLastPageIconNull && childGrid.querySelectorAll('.e-numericitem')) {
                    (childGrid.querySelectorAll('.e-numericitem')[childGrid.querySelectorAll('.e-numericitem').length - 1] as HTMLElement).focus();
                }
                return;
            } else if ((detailTemplateElements.children[parseInt(i.toString(), 10)] as HTMLElement).tabIndex === 0) {
                (detailTemplateElements.children[parseInt(i.toString(), 10)] as HTMLElement).focus();
                return;
            } else if (!isNullOrUndefined(detailTemplateElements.children[parseInt(i.toString(), 10)].children)
                && detailTemplateElements.children[parseInt(i.toString(), 10)].children.length !== 0) {
                this.iterateElements(detailTemplateElements.children[parseInt(i.toString(), 10)] as HTMLElement,
                                     childGrid as HTMLElement, isChildGridNull);
                if (!document.activeElement.classList.contains('e-detailcell')) { return; }

            }
        }
    },
    focusDetailTemplateElements(dataId: string, keyCombination: string, isDetailTemplateCell: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (isNullOrUndefined(gridInstance.element) && isNullOrUndefined(gridInstance.element.blazor__instance) && isNullOrUndefined(parentsUntil(document.activeElement, 'e-grid'))) {
            return;
        }
        const childGrid = document.activeElement.querySelector('.e-grid');
        const isChildGridNull = isNullOrUndefined(childGrid);
        if (keyCombination === 'Tab') {
            (document.activeElement as HTMLElement).blur();
        } else if (keyCombination === 'ShiftTab') {
            const detailTemplateElements = parentsUntil(document.activeElement, 'e-detailcell').firstElementChild;

            if (detailTemplateElements.children.length !== 0 && isDetailTemplateCell) {
                this.iterateElements(detailTemplateElements as HTMLElement, childGrid as HTMLElement, isChildGridNull);
            }

            if (document.activeElement.classList.contains('e-detailcell')) {
                const previousRowCells = document.activeElement.parentElement.previousElementSibling.querySelectorAll('.e-rowcell:not(.e-hide)');
                (previousRowCells[previousRowCells.length - 1] as HTMLElement).focus();
                return;
            }
        }

    },
    updateFilterBarCell(dataId: string, filteredFields: string[], filteredActualValue: string[]) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const filterRow = gridInstance.element.querySelector('.e-filterbar');
        if (isNullOrUndefined(filterRow)) { return; }
        filteredFields.forEach((fieldName, i) => {
            const filterCell = filterRow.querySelectorAll('#' + fieldName + '_filterBarcell');
            if (filterCell.length > 0) {
                filterCell[0].value = filteredActualValue[parseInt(i.toString(), 10)];
            }
        });
    },
    focusFilterBar(dataId: string, keyCombination: string, isFilterTemplate: boolean, index: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const filterRow = gridInstance.element.querySelector('.e-filterbar');
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)
            && !isNullOrUndefined(filterRow)) {
            if (keyCombination === 'Tab') {
                if (document.activeElement.classList.contains('e-headercell')) {
                    let filterBar = null;
                    // For enhanced filter bar - look for input in .e-input-group or .e-enhanced-filter-input
                    const enhancedFilterInput = filterRow.querySelector('.e-input-group input:not([disabled])');
                    if (!isNullOrUndefined(enhancedFilterInput)) {
                        filterBar = enhancedFilterInput;
                    } else {
                        // Fallback to old structure
                        filterBar = filterRow.querySelector('.e-textbox.e-input:not([disabled])');
                    }

                    if (isFilterTemplate) {
                        const filterInputDivs: NodeListOf<Element> = filterRow.querySelectorAll('.e-fltrinputdiv');
                        for (let i: number = 0; i < filterInputDivs.length; i++) {
                            // eslint-disable-next-line security/detect-object-injection
                            const divElement: Element = filterInputDivs[i];
                            if (divElement.children.length === 0) {
                                continue;
                            }
                            // Check for input in e-input-group (enhanced filter) first
                            const filterTemplateInput: HTMLElement | null = 
                                (divElement.querySelector('.e-input-group input') as HTMLElement) || 
                                (divElement.querySelector('[tabindex="0"]') as HTMLElement) || 
                                (divElement.querySelector('input') as HTMLElement);
                            if (!isNullOrUndefined(filterTemplateInput)) {
                                filterTemplateInput.focus();
                                break;
                            }
                            else {
                                (divElement.children[0] as HTMLElement).focus();
                                break;
                            }
                        }

                    } else if (!isNullOrUndefined(filterBar)) {
                        filterBar.focus();
                    }
                }
            }
            else if (keyCombination === 'ShiftTab') {
                const activeElement = document.activeElement as HTMLElement;
                const currentEditCell: Element = parentsUntil(activeElement, 'e-updatedtd');
                const isBatchEdit = gridInstance.options.editMode === 'Batch';
                const hasNoError =  currentEditCell &&  ((currentEditCell.querySelectorAll('.e-griderror')) as NodeListOf<Element>).length === 0;

                if (activeElement && activeElement.classList.contains('e-rowcell') || (isBatchEdit && hasNoError) || activeElement.classList.contains('e-recordplusexpand') || activeElement.classList.contains('e-recordpluscollapse')) {
                    let filterBar = null;
                    // For enhanced filter bar - look for inputs in .e-input-group
                    const enhancedFilterInputs = filterRow.querySelectorAll('.e-input-group input:not([disabled])');
                    if (enhancedFilterInputs.length > 0) {
                        filterBar = enhancedFilterInputs;
                    } else {
                        // Fallback to old structure
                        filterBar = filterRow.querySelectorAll('.e-textbox.e-input:not([disabled])');
                    }

                    if (isFilterTemplate) {
                        const filterElements = filterRow.querySelectorAll('.e-fltrinputdiv');
                        const lastFilterElement = filterElements[filterElements.length - 1];
                        // Check for input in e-input-group (enhanced filter) first
                        const lastFilterTemplate = 
                            (lastFilterElement.querySelector('.e-input-group input') as HTMLElement) ||
                            (lastFilterElement.querySelector('input') as HTMLElement);
                        if (!isNullOrUndefined(lastFilterTemplate)) {
                            lastFilterTemplate.focus();
                        } else {
                            (lastFilterElement.children[0] as HTMLElement).focus();
                        }
                    } else if (!isNullOrUndefined(filterBar) && (filterBar as any).length > 0) {
                        ((filterBar as any)[(filterBar as any).length - 1] as HTMLElement).focus();
                    }
                }
            }
            else if (keyCombination === 'ArrowUp' || keyCombination === 'ArrowDown') {
                if (document.activeElement.classList.contains('e-groupcaption') || document.activeElement.classList.contains('e-recordplusexpand')) {
                    filterRow.querySelector('.e-textbox.e-input:not([disabled])').focus();
                } else if (isFilterTemplate) {
                    const filterTemplateInput = filterRow.querySelectorAll('.e-fltrinputdiv')[index === -1 ? parseInt(document.activeElement.getAttribute('aria-colIndex'), 10) - 1 : index].querySelector('input');

                    if (!isNullOrUndefined(filterTemplateInput)) {
                        filterTemplateInput.focus();
                    } else {
                        filterRow.querySelectorAll('.e-fltrinputdiv')[index === -1 ? parseInt(document.activeElement.getAttribute('aria-colIndex'), 10) - 1 : index].children[0].focus();
                    }

                } else {
                    filterRow.querySelectorAll('.e-input')[index === -1 ? parseInt(document.activeElement.getAttribute('aria-colIndex'), 10) - 1 : index].focus();
                }
            }
        }
    },
    focusAddForm: function (dataId: string, keyCombination: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const showAddNewRow: HTMLTableRowElement = gridInstance.element.querySelector('.e-showAddNewRow');
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)
            && !isNullOrUndefined(showAddNewRow)) {
            if (keyCombination === 'Tab') {
                if ((document.activeElement as HTMLElement).classList.contains('e-headercell')) {
                    const inputTextBox: HTMLInputElement = showAddNewRow.querySelector('.e-input:not([disabled])');
                    if (!isNullOrUndefined(inputTextBox)) {
                        inputTextBox.focus();
                    }
                }
            }
            else if (keyCombination === 'ShiftTab') {
                if ((document.activeElement as HTMLElement).classList.contains('e-rowcell') || (document.activeElement as HTMLElement).classList.contains('e-recordplusexpand') || (document.activeElement as HTMLElement).classList.contains('e-recordpluscollapse')) {
                    const inputTextBox: NodeListOf<HTMLInputElement> = showAddNewRow.querySelectorAll('.e-input:not([disabled])');
                    const tds = showAddNewRow.querySelectorAll('.e-rowcell:not(.e-hide)');
                    const commonElements: HTMLElement[] = [];
                    tds.forEach(function (td) {
                        const inputInTd: HTMLElement = td.querySelector('.e-input:not([disabled])');
                        if (inputInTd && Array.from(inputTextBox).some(input => input === inputInTd)) {
                            commonElements.push(inputInTd);
                        }
                    });
                    if (!isNullOrUndefined(tds)) {
                        commonElements[commonElements.length - 1].focus();
                    }
                }
            }
            else if (keyCombination === 'ArrowUp' || keyCombination === 'ArrowDown') {
                if ((document.activeElement as HTMLElement).classList.contains('e-groupcaption') || (document.activeElement as HTMLElement).classList.contains('e-recordplusexpand') || (document.activeElement as HTMLElement).classList.contains('e-recordpluscollapse')) {
                    const inputTextBox: HTMLElement = showAddNewRow.querySelector('.e-input:not([disabled])');
                    if (!isNullOrUndefined(inputTextBox)) {
                        inputTextBox.focus();
                    }
                }
                else {
                    const colIndex = parseInt((document.activeElement as HTMLElement).getAttribute('aria-colIndex'), 10) - 1;
                    let index: number = 0;
                    if (colIndex !== null) {
                        index = colIndex;
                    }
                    const inputElement = showAddNewRow.querySelectorAll('.e-input')[parseInt(index.toString(), 10)] as HTMLInputElement;
                    inputElement.focus();
                }
            }
        }
    },
    focusFirstGroupHeader(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const firstGroupHeader = gridInstance.element.querySelector('.e-groupheadercell');
            if (!isNullOrUndefined(firstGroupHeader)) {
                firstGroupHeader.focus();
            }
        }
    },
    focusExcelInput(dataId: string, celluid: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const excelPopup: HTMLElement = document.querySelector('#' + celluid + '_excelDlg');
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)
            && !isNullOrUndefined(excelPopup)) {
            setTimeout(() => {
                const searchElement = excelPopup.querySelector('#' + gridInstance.element.id + '_SearchBox') as HTMLElement;
                if (!isNullOrUndefined(searchElement)) {
                    searchElement.focus();
                }
            }, 10);
        }
    },
    refreshOnDataChange(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance) &&
            !isNullOrUndefined(gridInstance.element.blazor__instance.virtualContentModule)) {
            gridInstance.element.blazor__instance.virtualContentModule.refreshOnDataChange();
        }
    },
    updateAutofillPosition(dataId: string, cellindex: number, index: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const _this = gridInstance.element.blazor__instance;
            return _this.selectionModule.updateAutofillPosition(cellindex, index);
        }
        else {
            return null;
        }
    },
    createBorder(dataId: string, rowIndex: number, cellIndex: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const _this = gridInstance.element.blazor__instance;
            return _this.selectionModule.createBorder(rowIndex, cellIndex);
        }
        else {
            return null;
        }
    },
    removePersistItem(dataId: string, id: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance) && !isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const _this: SfGrid = gridInstance.element.blazor__instance;
            (_this.getHeaderTable() as HTMLTableElement).style.width = '';
            (_this.getContentTable() as HTMLTableElement).style.width = '';
            if (_this.options.aggregatesCount !== 0 && !isNullOrUndefined(_this.getFooterContent().querySelector('.e-table'))) {
                (_this.getFooterContent().querySelector('.e-table') as HTMLTableElement).style.width = '';
            }
            if (_this.options.frozenColumns > 0) {
                const movableHeader: Element = _this.element.querySelector('.e-movableheader');
                const movableContent: Element = _this.element.querySelector('.e-movablecontent');
                if (!isNullOrUndefined(movableHeader) && !isNullOrUndefined(movableContent) &&
                    !isNullOrUndefined(movableHeader.querySelector('.e-table'))
                    && !isNullOrUndefined(movableContent.querySelector('.e-table'))) {
                    (_this.element.querySelector('.e-movableheader').querySelector('.e-table') as HTMLTableElement).style.width = '';
                    (_this.element.querySelector('.e-movablecontent').querySelector('.e-table') as HTMLTableElement).style.width = '';
                }
            }
        }
        localStorage.removeItem(id);
    },
    focusChild(dataId: string, rowuid: string, celluid: string, cellColIndex: number = -1) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        let cell: HTMLElement = getCellByRowUidAndColIndex(gridInstance.element, rowuid, cellColIndex);
        cell = cell == null ? gridInstance.element.querySelector('[data-uid=\'' + celluid + '\']') : cell;
        const childElements = cell.firstElementChild.children;
        const firstFocusableElement = gridInstance.iterateTemplateElementsForward(childElements);

        /* Select the first focusable child element
         * if no child found then select the cell itself.
         * if Grid is in editable state, check for editable control inside child.
         */

        if (!isNullOrUndefined(firstFocusableElement)) {
            firstFocusableElement.focus();
        } else {
            (<HTMLElement>gridInstance.element.querySelector('[data-uid="' + celluid + '"]')).focus();
        }
        return !isNullOrUndefined(firstFocusableElement) ? true : false;
    },
    exportSave(filename: string, bytesBase64: string) {
        var nav = navigator as any;
        if (nav.msSaveBlob) {
            //Download document in Edge browser
            const data: string = window.atob(bytesBase64);
            const bytes: Uint8Array = new Uint8Array(data.length);
            for (let i: number = 0; i < data.length; i++) {
                bytes[parseInt(i.toString(), 10)] = data.charCodeAt(i);
            }
            const blob = new Blob([bytes.buffer], { type: 'application/octet-stream' });
            nav.msSaveBlob(blob, filename);
        }
        else {
            const link: HTMLAnchorElement = document.createElement('a');
            link.download = filename;
            link.href = 'data:application/octet-stream;base64,' + bytesBase64;
            document.body.appendChild(link); // Needed for Firefox
            link.click();
            document.body.removeChild(link);
        }
    },
    destroy(dataId: string, isRerendered: boolean): void {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const gridInstance: any = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.destroy(isRerendered);
        }
    },

    validation(dataId: string, results: object[], isAdd: boolean, newRowPosition: string): void {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const gridInstance: any = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            if (!isNullOrUndefined(newRowPosition) && newRowPosition !== '' && gridInstance.options.newRowPosition !== newRowPosition) {
                gridInstance.options.newRowPosition = newRowPosition;
            }
            gridInstance.element.blazor__instance.editModule.createTooltip(results, isAdd);
        }
    },

    focusCell(dataId: string, field: string, isAdd: boolean, frozenEdit: boolean = false) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        let complexField: string = `#${field.replace(/[.]/g, '___')}`;
        complexField = complexField + ':not(.e-disabled)';
        if (isNullOrUndefined(gridInstance)) {
            return;
        }
        if (frozenEdit) {
            const forms: HTMLFormElement[] = [].slice.call(gridInstance.element.querySelectorAll('form'));
            let td: HTMLElement;
            for (let i: number = 0; i < forms.length; i++) {
                td = forms[parseInt(i.toString(), 10)].querySelector('td:not(.e-hide)');
                td.style.height = closest(td, '.e-row').getBoundingClientRect().height + 'px';
            }
        }
        if(!field)
        {
            return;
        }
        let complexElement: HTMLElement = gridInstance.element.querySelector(complexField);
        if (field !== '' && complexElement === null && complexField.includes('___')) {
            complexField = complexField.split('___').pop();
            complexElement = gridInstance.element.querySelector('#' + complexField);
        }
        if (field === '' && gridInstance.element.querySelector('input.e-boolcell')) {
            (gridInstance.element.querySelector('input.e-boolcell') as HTMLElement).focus();
        } else if (field !== '' && complexElement) {
            const lastAddedRow: boolean = (gridInstance.getContent().querySelector('tbody') as HTMLElement).classList.contains('e-addedrow');
            const preventScrolling: boolean = gridInstance.options.enableVirtualization && gridInstance.options.allowEditing
            && gridInstance.options.isEdit && !lastAddedRow;
            complexElement.focus({ preventScroll: preventScrolling });
        }
    },

    CurrentPageFocus(dataId: string, key: string, currentPage: string) {
        const pagerInstance = this.sfBlazor.getCompInstance(dataId);
        const numericContainer = pagerInstance.element.querySelector('.e-numericcontainer');
        if (key === 'PreviousPage' || (numericContainer.querySelectorAll('.e-link:last-child')[0] as HTMLElement).innerText !== currentPage) {
            (numericContainer.querySelector('.e-link') as HTMLElement).focus();
        } else {
            (numericContainer.querySelectorAll('.e-link:last-child')[0] as HTMLElement).focus();
        }
    },

    pagerFocus(dataId: string, key: string) {
        const pagerInstance = this.sfBlazor.getCompInstance(dataId);
        const pagerContainer: HTMLElement = pagerInstance.element.querySelector('.e-gridpager').querySelector('.e-pagercontainer');
        const numericContainer: HTMLElement = pagerContainer.querySelector('.e-numericcontainer');
        const firstPage: HTMLElement = pagerContainer.querySelector('.e-firstpage.e-pager-default');
        const previousPage: HTMLElement = pagerContainer.querySelector('.e-prevpage.e-pager-default');
        if (key === 'ArrowDown') {
            if (firstPage) {
                (firstPage as HTMLElement).focus();
                return 'FirstPage';
            } else if (previousPage) {
                (firstPage as HTMLElement).focus();
                return 'PreviousPage';
            } else {
                (numericContainer.querySelectorAll('.e-link')[1] as HTMLElement).focus();
                return '1';
            }
        } else if (key === 'ArrowRight') {
            if (firstPage !== null && firstPage.classList.contains('e-focused')) {
                (previousPage as HTMLElement).focus();
                return 'PreviousPage';
            } else if (previousPage != null && previousPage.classList.contains('e-focused') || pagerContainer.querySelector('.e-pp.e-focused') != null) {
                if (pagerContainer.querySelector('.e-pp') != null && !pagerContainer.querySelector('.e-pp').classList.contains('e-focused')) {
                    (pagerContainer.querySelector('.e-pp') as HTMLElement).focus();
                    return 'PreviousPagerCount';
                } else {
                    (numericContainer.querySelectorAll('.e-link')[0] as HTMLElement).focus();
                    return (numericContainer.querySelectorAll('.e-link')[0] as HTMLElement).innerText;
                }
            } else if (numericContainer.querySelectorAll('.e-link.e-focused').length > 0 && pagerContainer.querySelector('.e-link.e-focused') != null && pagerContainer.querySelector('.e-link.e-focused').nextElementSibling != null) {
                (numericContainer.querySelector('.e-link.e-focused').nextElementSibling as HTMLElement).focus();
                return (numericContainer.querySelector('.e-link.e-focused').nextElementSibling as HTMLElement).innerText;
            } else if (numericContainer.querySelectorAll('.e-link.e-focused').length > 0 && pagerContainer.querySelector('.e-np') != null && pagerContainer.querySelector('.e-np.e-focused') == null) {
                (pagerContainer.querySelector('.e-np') as HTMLElement).focus();
                return 'NextPagerCount';
            } else if (numericContainer.querySelectorAll('.e-link.e-focused').length > 0 || pagerContainer.querySelectorAll('.e-np.e-focused').length > 0) {
                if (pagerContainer.querySelector('.e-nextpage') != null) {
                    (pagerContainer.querySelector('.e-nextpage') as HTMLElement).focus();
                    return 'NextPage';
                } else {
                    (numericContainer.querySelector('.e-link.e-focused') as HTMLElement).focus();
                    return (numericContainer.querySelector('.e-link.e-focused') as HTMLElement).innerText;
                }
            } else if (pagerContainer.querySelector('.e-nextpage.e-focused') != null) {
                (pagerContainer.querySelector('.e-lastpage') as HTMLElement).focus();
                return 'LastPage';
            } else {
                (pagerContainer.querySelector('.e-lastpage') as HTMLElement).focus();
                return 'LastPage';
            }
        }
        else if (key === 'ArrowLeft') {
            if (previousPage != null && previousPage.classList.contains('e-focused')) {
                (firstPage as HTMLElement).focus();
                return 'FirstPage';
            } else if (previousPage && pagerContainer.querySelector('.e-pp.e-focused')) {
                (previousPage as HTMLElement).focus();
                return 'PreviousPage';
            } else if (numericContainer.querySelectorAll('.e-link')[0].classList.contains('e-focused')) {
                if (pagerContainer.querySelector('.e-pp') != null) {
                    (pagerContainer.querySelector('.e-pp') as HTMLElement).focus();
                    return 'PreviousPagerCount';
                } else if (previousPage) {
                    (previousPage as HTMLElement).focus();
                    return 'PreviousPage';
                } else {
                    (numericContainer.querySelectorAll('.e-link')[0] as HTMLElement).focus();
                    return '1';
                }
            } else if (numericContainer.querySelectorAll('.e-link.e-focused').length > 0) {
                (numericContainer.querySelector('.e-link.e-focused').previousElementSibling as HTMLElement).focus();
                return (numericContainer.querySelector('.e-link.e-focused').previousElementSibling as HTMLElement).innerText;
            } else if (pagerContainer.querySelectorAll('.e-nextpage.e-focused').length > 0 && pagerContainer.querySelector('.e-np') != null) {
                (pagerContainer.querySelector('.e-np') as HTMLElement).focus();
                return 'NextPagerCount';
            } else if (pagerContainer.querySelectorAll('.e-nextpage.e-focused').length > 0 || pagerContainer.querySelectorAll('.e-np.e-focused').length > 0) {
                const page = numericContainer.querySelectorAll('.e-link').length;
                (numericContainer.querySelectorAll('.e-link')[page - 1] as HTMLElement).focus();
                return (numericContainer.querySelectorAll('.e-link:last-child')[0] as HTMLElement).innerText;
            } else if (pagerContainer.querySelector('.e-lastpage.e-focused') !== null) {
                (pagerContainer.querySelector('.e-nextpage') as HTMLElement).focus();
                return 'NextPage';
            } else {
                if (!firstPage.classList.contains('.e-disabled')) {
                    (firstPage as HTMLElement).focus();
                    return 'FirstPage';
                }
                return '0';
            }
        }
        else { return '0'; }
    },

    setFrozenHeight(element: BlazorGridElement) {
        (element.querySelector('.e-frozencontent') as HTMLElement).style.height =
            (element.querySelector('.e-movablecontent') as HTMLElement).offsetHeight - getScrollBarWidth() + 'px';
    },

    printGrid(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.print();
        }
    },
    updateMediaColumns(dataId: string, mediaColumnsUid: { [uid: string]: boolean }) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.columnChooserModule.updateMediaColumns(mediaColumnsUid);
        }
    },
    copyToClipBoard(dataId: string, withHeader?: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.clipboardModule.copy(withHeader);
        }
    },
    preventCopyToClipBoard(dataId: string, cancelValue: boolean, value: string, action: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const gridclipBoardElement = gridInstance.clipboardModule;
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            if (action === 'Copy') {
                gridclipBoardElement.clipBoardData(cancelValue, value);
            }
            else if (action === 'Paste') {
                gridclipBoardElement.pasteAction(value, gridclipBoardElement.getSelectedRowCellIndexes()[0].rowIndex,
                                                 gridclipBoardElement.getSelectedRowCellIndexes()[0].cellIndexes[0], cancelValue);
            }
        }
    },
    preventPasteAction(dataId: string, rowIndex: number, columnField: string, value: string, columnIndex: number, cancelValue: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.clipboardModule.pasteData(rowIndex, columnField, value, columnIndex, cancelValue);
        }
    },
    setMediaColumns: function setMediaColumns(dataId: string, isResetPersistData?: boolean) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            gridInstance.element.blazor__instance.columnChooserModule.setMediaColumns(isResetPersistData);
        }
    },
    gridFocus(dataId: string, isFocusGrid: boolean = false, isLastBatchEditCell: boolean = false, keyCombination: string = '') {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance) && !isNullOrUndefined(gridInstance.element) && !isLastBatchEditCell) {
            const groupHeaderCells = gridInstance.element.querySelectorAll('.e-groupheadercell').length;
            if (gridInstance.element.querySelectorAll('.e-toolbar-item').length !== 0) {
                const visibleToolbarItems = gridInstance.element.querySelectorAll('.e-toolbar-item:not(.e-overlay)');
                let focusElement = null;
                if (!isNullOrUndefined(visibleToolbarItems) && visibleToolbarItems.length !== 0) {
                    const lastToolbarIndex: number = visibleToolbarItems.length - 1;
                    if (!isNullOrUndefined(visibleToolbarItems[parseInt(lastToolbarIndex.toString(), 10)].querySelector('.e-tbar-btn'))) {
                        for (let i: number = lastToolbarIndex; i >= 0; i--) {
                            if (!isNullOrUndefined(visibleToolbarItems[parseInt(i.toString(), 10)].querySelector('.e-tbar-btn')) && visibleToolbarItems[parseInt(i.toString(), 10)].querySelector('.e-tbar-btn').tabIndex === 0) {
                                focusElement = visibleToolbarItems[parseInt(i.toString(), 10)].querySelector('.e-tbar-btn');
                                break;
                            }
                        }
                        if (!isNullOrUndefined(focusElement)){
                            focusElement.focus();
                            return;
                        }
                    } else if (!isNullOrUndefined(visibleToolbarItems[parseInt(lastToolbarIndex.toString(), 10)].querySelector('.e-searchinput.e-input'))) {
                        visibleToolbarItems[parseInt(lastToolbarIndex.toString(), 10)].querySelector('.e-searchinput.e-input').focus();
                        return;
                    }
                }

            } else if (groupHeaderCells > 0 && (document.activeElement.classList.contains('e-headercell') || document.activeElement.classList.contains('e-recordplusexpand') || document.activeElement.classList.contains('e-recordpluscollapse'))) {
                const unGroupButtons = gridInstance.element.querySelectorAll('.e-ungroupbutton');
                unGroupButtons[unGroupButtons.length - 1].focus();
                return;
            } else if (groupHeaderCells === 0 && gridInstance.options.allowGrouping && document.activeElement.classList.contains('e-headercell')) {
                gridInstance.element.querySelector('.e-groupdroparea').focus();
                return;
            } else if (keyCombination !== null && keyCombination === 'ShiftTab') { // Ensure next focusable element in DOM when Shift+Tab is pressed
                /* eslint-disable @typescript-eslint/no-explicit-any */
                const gridElement: any = gridInstance.element;
                const focusableSelectors: string =
                    'a[href], area[href], input:not([disabled]), select:not([disabled]), ' +
                    'textarea:not([disabled]), button:not([disabled]), iframe, object, embed, ' +
                    '[tabindex], [contenteditable]';

                const allFocusableElementsInDom: any = Array.prototype.slice
                    .call(document.querySelectorAll(focusableSelectors))
                    .filter(function (el: any): boolean {
                        const isVisible: boolean = el.offsetParent !== null;
                        return isVisible && !gridElement.contains(el);
                    });

                // Find the first focusable element before the Grid in DOM order
                const beforeGridElements: any =
                    allFocusableElementsInDom.filter(function (el: any): boolean {
                        return !!(
                            gridElement.compareDocumentPosition(el) &
                            Node.DOCUMENT_POSITION_PRECEDING
                        );
                    });
                if (!isNullOrUndefined(beforeGridElements) && beforeGridElements.length > 0) {
                    beforeGridElements[beforeGridElements.length - 1].focus();
                    return;
                }
            }

            if (parentsUntil(document.activeElement, 'e-grid') || (isFocusGrid && !isNullOrUndefined(gridInstance))) {
                gridInstance.element.focus();
            }
        }
    },

    isMacDevice() {
        return navigator.userAgent.indexOf('Mac OS') !== -1;
    },

    updateClonedMaskTranslates(dataId: string) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const gridInstance: any = this.sfBlazor.getCompInstance(dataId);
        const gObj: SfGrid = gridInstance.element.blazor__instance;
        const gridContent = gObj.getContent();
        const maskedTable: HTMLElement = gridContent.querySelector('.e-masked-table');
        const minScrollTop: number = gridContent.scrollHeight - maskedTable.getBoundingClientRect().height;
        const scrollTop: number = gridContent.scrollTop <= minScrollTop ? gridContent.scrollTop : minScrollTop;
        maskedTable.style.transform = 'translate(0px,' + scrollTop + 'px)';
    },

    refreshScrollLeftPosition: function refreshScrollLeftPosition(dataId: string): void {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const gridInstance: any = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const gObj: SfGrid = gridInstance.element.blazor__instance;
            const scrollContent: HTMLElement = gObj.getContent();
            const scrollLeft: number = scrollContent.scrollLeft;
            scrollContent.scrollLeft = scrollLeft + 20;
        }
    },

    refreshGridPageSize(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance) && !isNullOrUndefined(gridInstance.scrollModule)) {
            gridInstance.scrollModule.refresh();
        }
    },

    scrollIntoView(dataId: string, columnIndex: number, rowIndex: number, rowHeight: number, isAddBottom: boolean = false,
                   isFromAddForm: boolean = false) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const gObj: SfGrid = gridInstance.element.blazor__instance;
        const scrollContent = gObj.getContent();
        const prevScrollTop: number = scrollContent.scrollTop;
        const rowHeightValue: number = rowHeight !== -1 ? rowHeight : gObj.getRowHeight();
        gObj.virtualContentModule.focusColumnIndex = columnIndex;
        if (isAddBottom) {
            scrollContent.scrollTop = scrollContent.scrollHeight;
        }
        else if (rowIndex !== -1) {
            scrollContent.scrollTop = rowIndex * rowHeightValue;
            gObj.virtualContentModule.selectedRowIndex = rowIndex;
            gObj.virtualContentModule.isScrollIntoview = true;
            if (!isFromAddForm && ((rowIndex === 0 && prevScrollTop === 0) || ((prevScrollTop === scrollContent.scrollTop) && !((gObj.virtualContentModule.scrollInfo.direction === 'left') || (gObj.virtualContentModule.scrollInfo.direction === 'right'))))) {
                gObj.dotNetRef.invokeMethodAsync('SelectRow', rowIndex, gObj.virtualContentModule.isScrollIntoview, gObj.virtualContentModule.focusColumnIndex);
                gObj.virtualContentModule.selectedRowIndex = -1;
            }
            else if (rowIndex >= gObj.options.totalItemCount) {
                gObj.virtualContentModule.selectedRowIndex = gObj.options.totalItemCount - 1;
            }
        }
        const columnOffsets = gObj.virtualContentModule.vHelper.cOffsets;
        const colOffsets = gObj.nColumnOffsets;
        setTimeout(function () {
            if (!gObj.options.frozenColumns && columnIndex !== -1) {
                if (gObj.options.enableColumnVirtualization) {
                    scrollContent.scrollLeft = columnOffsets[columnIndex - 1];
                }
                else {
                    scrollContent.scrollLeft = colOffsets[columnIndex - 1];
                }
            }
            if (gObj.options.enableColumnVirtualization && columnIndex > 0) {
                if (gObj.options.frozenColumns) {
                    const customIndex = gObj.options.frozenLeftCount > 0 ? gObj.options.frozenLeftCount : gObj.options.frozenColumns;
                    scrollContent.scrollLeft = columnOffsets[parseInt(customIndex.toString(), 10) - 1];
                }
            }
            if (gObj.options.enableVirtualMaskRow && gObj.virtualContentModule.startIndex === 0) {
                const virtualTable: HTMLElement = gObj.content.querySelector('.e-virtualtable') as HTMLElement;
                const translateY: number = (gObj.virtualContentModule.startIndex * rowHeightValue)
                    - (gObj.options.pageSize * rowHeightValue);
                virtualTable.style.transform = 'translate(0px,' + translateY + 'px)';
            }
        }, 20);
    },

    lazyLoadGridHeight(dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        let height: number = 0;
        if (!isNullOrUndefined(gridInstance.getContent())) {
            height = gridInstance.getContent().offsetHeight;
        }
        return height;
    },

    resetExpandCollapseAllScroll(dataId: string, requestType: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        gridInstance.options.requestType = requestType;
        if (!isNullOrUndefined(gridInstance)) {
            gridInstance.infiniteScrollModule.resetInfniniteScrollPositions();
        }
    },

    updateResizeCursor: function (dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance)) {
            gridInstance.resizeModule.updateHelper();
        }
    },

    getContentCell: function (dataId: string , top: number , left: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance)) {
            return gridInstance.getContentCell(gridInstance.element , top , left);
        }
    },
    focusNextFrame: function (dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        return new Promise((resolve) => {
            requestAnimationFrame(() => {
                requestAnimationFrame(() => resolve(true));
            });
        });
    },
    forceUpdateTranslate: function (dataId: string, translateY: number) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        const gridElement = gridInstance.element;
        if (!isNullOrUndefined(gridElement)) {
            const gridBlazorInstance = gridElement.blazor__instance;
            if (!isNullOrUndefined(gridBlazorInstance) && !isNullOrUndefined(gridBlazorInstance.virtualContentModule)) {
                gridBlazorInstance.virtualContentModule.virtualEle.adjustTable(0, translateY);
            }
        }
    },
    syncTableWidthsAfterReset: function (dataId: string) {
        const gridInstance = this.sfBlazor.getCompInstance(dataId);
        if (!isNullOrUndefined(gridInstance) && !isNullOrUndefined(gridInstance.element) && !isNullOrUndefined(gridInstance.element.blazor__instance)) {
            const _this: SfGrid = gridInstance.element.blazor__instance;
            const contentTable: HTMLElement = _this.getContentTable() as HTMLElement;
            const headerTable: HTMLElement = _this.getHeaderTable() as HTMLElement;
            if (!isNullOrUndefined(contentTable) && !isNullOrUndefined(headerTable)) {
                const isResized: NodeListOf<HTMLElement> = headerTable.querySelectorAll('.e-resized');
                if (!isNullOrUndefined(isResized) && isResized.length > 0 && headerTable.offsetWidth > 0 && contentTable.offsetWidth > 0) {
                    contentTable.style.width = headerTable.offsetWidth + 'px';
                    _this.isResetDataTriggered = true;
                }
            }
        }
    },
    IsInputfocus: function IsInputfocus()
    {
        setTimeout(function ()
        {
            var activeElement = document.activeElement;
            var isDropdownActive = activeElement.classList.contains('e-enhanced-operator-dropdown') ||
                activeElement.closest('span.e-enhanced-operator-dropdown') !== null;
            if (activeElement && isDropdownActive)
            {
                var tdElement = activeElement.closest('td.e-filterbarcell');
                if (tdElement)
                {
                        var inputElement = tdElement.querySelector('.e-enhanced-filter-input input.e-input') as HTMLElement;
                        if (inputElement)
                        {
                            inputElement.focus();
                        }
                    
                } 
            }
        }, 100);
    },
    getIsDeviceMode: function (): boolean {
        try {
            // Access sf.base.Browser.isDevice which is available globally
            if (typeof (window as any).sf !== 'undefined' && 
                typeof (window as any).sf.base !== 'undefined' && 
                typeof (window as any).sf.base.Browser !== 'undefined') {
                return (window as any).sf.base.Browser.isDevice;
            }
        } catch (error) {
            console.warn('Error detecting device mode:', error);
        }
        return false;
    }
};

export default Grid;
