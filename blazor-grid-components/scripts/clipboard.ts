import { Browser, isUndefined, createElement, closest, classList } from '@syncfusion/ej2-base';
import { parentsUntil } from './util';
import { SfGrid } from './sf-grid-fn';
import { Column, ISelectedCell } from './interfaces';

/**
 * The `Clipboard` module is used to handle clipboard copy action.
 */
export class Clipboard {
    //Internal variables
    private activeElement: Element;
    protected clipBoardTextArea: HTMLInputElement;
    private copyContent: string = '';
    private isSelect: boolean = false;
    //Module declarations
    private parent: SfGrid;

    constructor(parent?: SfGrid) {
        this.parent = parent;
        this.clipBoardTextArea = createElement('textarea', {
            className: 'e-clipboard',
            styles: 'opacity: 0',
            attrs: { tabindex: '-1', 'aria-label': 'clipboard' }
        }) as HTMLInputElement;
        this.parent.element.appendChild(this.clipBoardTextArea);
    }

    public pasteHandler(): void {
        const grid: SfGrid = this.parent;
        const target: HTMLElement = closest(document.activeElement, '.e-rowcell') as HTMLElement;
        if (!target || !grid.options.allowEditing || grid.options.editMode !== 'Batch' ||
            grid.options.selectionMode !== 'Cell' || grid.options.cellSelectionMode === 'Flow' ||
            (target.classList.contains('e-editedbatchcell') && grid.options.cellSelectionMode === 'Box')) {
            return;
        }
        this.activeElement = document.activeElement;
        this.clipBoardTextArea.value = '';
        const x: number = window.scrollX;
        const y: number = window.scrollY;
        this.clipBoardTextArea.focus();
        setTimeout(
            () => {
                (this.activeElement as HTMLInputElement).focus();
                window.scrollTo(x, y);
                const name: string = 'Paste';
                grid.dotNetRef.invokeMethodAsync('InvokeCopyPasteAction', {
                    clipboardText: this.clipBoardTextArea.value
                }, name);
            }, 10);
    }

    public pasteAction(data: string, rowIndex: number, colIndex: number, cancel: boolean): void {
        if (!cancel && data !== '') {
            this.paste(data, rowIndex, colIndex);
        }
    }

    public paste(data: string, rowIndex: number, colIndex: number): void {
        const grid: SfGrid = this.parent;
        let cIdx: number = colIndex;
        let rIdx: number = rowIndex;
        let col: Column;
        let value: string;
        let isAvail: Element | boolean;
        if (!grid.options.allowEditing || grid.options.editMode !== 'Batch' ||
            grid.options.selectionMode !== 'Cell' || grid.options.cellSelectionMode === 'Flow') {
            return;
        }
        // Check if the data represents a single cell with multiline content
        const isSingleCellMultiline: boolean = data.trim().startsWith('"') && data.trim().endsWith('"');

        if (isSingleCellMultiline) {
            // Handle single cell multiline paste
            const pasteData: string = data.trim().slice(1, -1).replace(/\\n/g, '\n'); // Remove quotes and decode newlines
            const column: Column = grid.getColumnByIndex(cIdx);
            this.invokePasteAction(rIdx, cIdx, pasteData, column);
            return;
        }
        const rows: string[] = data.split('\n');
        let cols: string[];
        const dataRows: HTMLElement[] = grid.getDataRows() as HTMLElement[];
        let mRows: HTMLElement[];
        const isFrozen: number = this.parent.options.frozenColumns;
        if (isFrozen) {
            mRows = grid.getMovableDataRows() as HTMLElement[];
        }

        for (let r: number = 0; r < rows.length; r++) {
            cols = rows[parseInt(r.toString(), 10)].split('\t');
            cIdx = colIndex;
            if ((r === rows.length - 1 && rows[parseInt(r.toString(), 10)] === '') || isUndefined(grid.getRowByIndex(rIdx))) {
                cIdx++;
                break;
            }
            for (let c: number = 0; c < cols.length; c++) {
                isAvail = grid.getCellFromIndex(rIdx, cIdx);
                if (isFrozen) {
                    const fTr: HTMLElement = dataRows[parseInt(rIdx.toString(), 10)] as HTMLElement;
                    const mTr: HTMLElement = mRows[parseInt(rIdx.toString(), 10)] as HTMLElement;
                    isAvail = !fTr.querySelector('[aria-colindex="' + (cIdx + 1) + '"]') ?
                        mTr.querySelector('[aria-colindex="' + (cIdx + 1) + '"]') : true;
                }
                if (!isAvail) {
                    cIdx++;
                    break;
                }
                col = grid.getColumnByIndex(cIdx);
                value = cols[parseInt(c.toString(), 10)];
                this.invokePasteAction(rIdx, cIdx, value, col);
                cIdx++;
            }
            rIdx++;
        }
    }

    private invokePasteAction(rowIndex: number, columnIndex: number, pasteValue: string, column: Column): void {
        const grid: SfGrid = this.parent;
        if (column.allowEditing && !column.isPrimaryKey && !column.template) {
            grid.dotNetRef.invokeMethodAsync('InvokePasteAction', {
                cellValue: pasteValue
            }, rowIndex, columnIndex, column.field);
        }
    }

    public pasteData(rowIndex: number, columnField: string, value: string, columnIndex: number, cancel: boolean): void {
        if (!cancel) {
            if (this.parent.editModule) {
                {
                    this.parent.dotNetRef.invokeMethodAsync('UpdateCell', rowIndex, columnField, value);
                }
            }
            const cell: Element = this.parent.getCellFromIndex(rowIndex, columnIndex);
            if (cell) {
                classList(cell, ['e-focus', 'e-focused'], []);
            }
        }
    }

    protected setCopyData(withHeader?: boolean): void {
        if (window.getSelection().toString() === '') {
            this.clipBoardTextArea.value = this.copyContent = '';
            const rows: Element[] = this.parent.getRows();
            if (this.parent.options.selectionMode !== 'Cell') {
                //let selectedIndexes: Object[] = this.parent.getSelectedRowIndexes().sort((a: number, b: number) => { return a - b; });
                const selectedIndexes: number[] = this.parent.getSelectedRowIndexes(this.parent.options.enableVirtualization);
                if (withHeader) {
                    const headerTextArray: string[] = [];
                    for (let i: number = 0; i < this.parent.getVisibleColumns().length; i++) {
                        headerTextArray[parseInt(i.toString(), 10)] =
                        this.parent.getVisibleColumns()[parseInt(i.toString(), 10)].headerText;
                    }
                    this.getCopyData(headerTextArray, false, '\t', withHeader);
                    this.copyContent += '\n';
                }
                for (let i: number = 0; i < selectedIndexes.length; i++) {
                    if (i > 0) {
                        this.copyContent += '\n';
                    }
                    if (parseInt(rows[0].getAttribute('aria-rowindex'), 10) !== 1 && this.parent.options.selectionMode !== 'Both') {
                        selectedIndexes[parseInt(i.toString(), 10)] = selectedIndexes[parseInt(i.toString(), 10)] - parseInt(rows[0].getAttribute('aria-rowindex'), 10) + 1;
                    }
                    const cells: HTMLElement[] = [].slice.call(rows[selectedIndexes[parseInt(i.toString(), 10)] as number].querySelectorAll('.e-rowcell:not(.e-hide)'));
                    this.getCopyData(cells, false, '\t', withHeader);
                }
            } else {
                const obj: { status: boolean, rowIndexes?: number[], colIndexes?: number[] } = this.checkBoxSelection();
                if (obj.status) {
                    if (withHeader) {
                        const headers: HTMLElement[] = [];
                        for (let i: number = 0; i < obj.colIndexes.length; i++) {
                            headers.push(this.parent.getColumnHeaderByIndex(obj.colIndexes[parseInt(i.toString(), 10)]) as HTMLElement);
                        }
                        this.getCopyData(headers, false, '\t', withHeader);
                        this.copyContent += '\n';
                    }
                    for (let i: number = 0; i < obj.rowIndexes.length; i++) {
                        if (i > 0) {
                            this.copyContent += '\n';
                        }
                        const cells: HTMLElement[] = [].slice.call(rows[obj.rowIndexes[parseInt(i.toString(), 10)] as number].
                            querySelectorAll('.e-cellselectionbackground'));
                        this.getCopyData(cells, false, '\t', withHeader);
                    }
                } else {
                    this.getCopyData(
                        [].slice.call(this.parent.element.querySelectorAll('.e-cellselectionbackground')),
                        true, '\n', withHeader);
                }
            }
            const name: string = 'Copy';
            if (this.parent.options.isClipboardEventBinded) {
                this.parent.dotNetRef.invokeMethodAsync('InvokeCopyPasteAction', {
                    clipboardText: this.copyContent
                }, name);
            } else {
                this.clipBoardData(false, this.copyContent);
            }
        } else if (parentsUntil(document.activeElement, 'e-grid')) {
            const clipboardFocus: Element | null = document.activeElement;
            const selectedText: string = window.getSelection().toString();
            if (selectedText) {
                (navigator as any).clipboard.writeText(selectedText).then(() => {
                    this.clipBoardTextArea.blur();
                    (clipboardFocus as HTMLElement).focus();
                });
            }
        }
    }

    private clipBoardData(cancel: boolean, data: string): void {
        const clipboardFocus: Element = document.activeElement;
        this.copyContent = data !== '' ? data : this.copyContent;
        if (!cancel) {
            this.clipBoardTextArea.value = this.copyContent;
            if (!Browser.userAgent.match(/ipad|ipod|iphone/i)) {
                this.clipBoardTextArea.select();
            } else {
                this.clipBoardTextArea.setSelectionRange(0, this.clipBoardTextArea.value.length);
            }
            this.isSelect = true;
            if ((navigator as any).clipboard) {
                (navigator as any).clipboard.writeText(this.copyContent).then(() => {
                    this.clipBoardTextArea.blur();
                    (clipboardFocus as HTMLElement).focus({ preventScroll: true });
                });
            }
            if (this.isSelect) {
                window.getSelection().removeAllRanges();
                this.isSelect = false;
            }
        }
    }

    private getCopyData(cells: HTMLElement[] | string[], isCell: boolean, splitKey: string, withHeader?: boolean): void {
        const isElement: boolean = typeof cells[0] !== 'string';
        for (let j: number = 0; j < cells.length; j++) {
            if (withHeader && isCell) {
                this.copyContent += (this.parent.getColumns() as Column[])[parseInt((cells[parseInt(j.toString(), 10)] as HTMLElement).getAttribute('aria-colindex'), 10) - 1].headerText + '\n';

            }
            if (isElement) {
                if (!(cells[parseInt(j.toString(), 10)] as HTMLElement).classList.contains('e-hide')) {
                    if ((!(cells[parseInt(j.toString(), 10)] as HTMLElement).classList.contains('e-gridchkbox')) &&
                        Object.keys((cells[parseInt(j.toString(), 10)] as HTMLElement).querySelectorAll('.e-check')).length) {
                        this.copyContent += true;
                    } else if ((!(cells[parseInt(j.toString(), 10)] as HTMLElement).classList.contains('e-gridchkbox')) &&
                        Object.keys((cells[parseInt(j.toString(), 10)] as HTMLElement).querySelectorAll('.e-uncheck')).length) {
                        this.copyContent += false;
                    } else {
                        this.copyContent += (cells[parseInt(j.toString(), 10)] as HTMLElement).innerText;
                    }
                }
            } else {
                this.copyContent += cells[parseInt(j.toString(), 10)];
            }
            if (j < cells.length - 1) {
                this.copyContent += splitKey;
            }
        }
    }

    public copy(withHeader?: boolean): void {
        if ((navigator as any).clipboard) {
            this.setCopyData(withHeader);
        }
    }

    private getSelectedRowCellIndexes(): ISelectedCell[] {
        const gridObj: SfGrid = this.parent;
        const rowCellIndxes: ISelectedCell[] = [];
        const rows: Element[] = gridObj.getRows();
        let mrows: Element[];
        if (gridObj.options.frozenColumns) {
            mrows = gridObj.getMovableDataRows();
        }
        for (let i: number = 0; i < rows.length; i++) {
            let tempCells: NodeList = rows[parseInt(i.toString(), 10)].querySelectorAll('.e-cellselectionbackground');
            if (gridObj.options.frozenColumns && !tempCells.length) {
                tempCells = mrows[parseInt(i.toString(), 10)].querySelectorAll('.e-cellselectionbackground');
            }
            if (tempCells.length) {
                const cellIndexes: number[] = [];
                tempCells.forEach((element: HTMLElement) => {
                    cellIndexes.push(parseInt(element.getAttribute('aria-colindex'), 10) - 1);
                });
                rowCellIndxes.push({ rowIndex: i, cellIndexes: cellIndexes });
            }
        }
        return rowCellIndxes;
    }

    private checkBoxSelection(): { status: boolean, rowIndexes?: number[], colIndexes?: number[] } {
        const gridObj: SfGrid = this.parent;
        let rowCellIndxes: ISelectedCell[];
        let obj: { status: boolean, rowIndexes?: number[], colIndexes?: number[] } = { status: false };
        if (gridObj.options.selectionMode === 'Cell') {
            rowCellIndxes = this.getSelectedRowCellIndexes();
            let str: string;
            const rowIndexes: number[] = [];
            let i: number;
            for (i = 0; i < rowCellIndxes.length; i++) {
                if (rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.length) {
                    rowIndexes.push(rowCellIndxes[parseInt(i.toString(), 10)].rowIndex);
                }
                if (rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.length) {
                    if (!str) {
                        str = JSON.stringify(rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.sort());
                    }
                    if (str !== JSON.stringify(rowCellIndxes[parseInt(i.toString(), 10)].cellIndexes.sort())) {
                        break;
                    }
                }
            }
            rowIndexes.sort((a: number, b: number) => { return a - b; });
            if (i > 0 && i === rowCellIndxes.length) {
                obj = { status: true, rowIndexes: rowIndexes, colIndexes: rowCellIndxes[0].cellIndexes };
            }
        }
        return obj;
    }
}
