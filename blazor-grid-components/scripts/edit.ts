import { closest, isNullOrUndefined } from '@syncfusion/ej2-base';
import { calculateRelativeBasedPosition, OffsetPosition } from '@syncfusion/ej2-popups';
import { SfGrid } from './sf-grid-fn';
import { getScrollBarWidth , parentsUntil } from './util';
import { Column } from './interfaces';

/**
 * Editing
 */
export class Edit {

    private parent: SfGrid;
    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    public createTooltip(results: object[], isAdd: boolean): void {
        const toolTipPos: object = {};
        let arrowPosition: string;
        let element: HTMLElement;
        let isHeader: boolean = false;
        for (let i: number = 0; i < results.length ; i++ ) {
            const gcontent: HTMLElement = this.parent.getContent() as HTMLElement;
            let name: string = results[`${i}`]['fieldName']; const uid: string = results[`${i}`]['uid']; const message: string = results[`${i}`]['message'];          name = name.replace(/[.]/g, '___');
            if (this.parent.options.hasTemplateInEditSettings || isNullOrUndefined(uid)) {
                element = this.parent.element.querySelector(`#${name}`) || document.querySelector(`#${name}`) as HTMLElement;
                if (this.parent.options.isRenderedFromTreeGrid) {
                    if (((this.parent.element.querySelector(`#DataItem___${name}`) as HTMLElement) != null) || ((document.querySelector(`#DataItem___${name}`) as HTMLElement) != null)) {
                        element = this.parent.element.querySelector(`#DataItem___${name}`) as HTMLElement || document.querySelector(`#DataItem___${name}`) as HTMLElement;
                    }
                    else {
                        element = this.parent.element.querySelector(`#${name}`) || document.querySelector(`#${name}`) as HTMLElement;
                    }
                }
            }
            else {
                if (document.querySelectorAll('[e-mappinguid=' + uid + '_Dialog' + ']')[0] as HTMLElement) {
                    element = document.querySelectorAll('[e-mappinguid=' + uid + '_Dialog' + ']')[0] as HTMLElement;
                }
                else if ((this.parent.options.enableVirtualization || this.parent.options.frozenRows ||
                    this.parent.options.enableInfiniteScrolling) && this.parent.options.showAddNewRow) {
                    const tdElements: NodeListOf<HTMLElement> = this.parent.element.querySelectorAll('[e-mappinguid=' + uid + '].e-rowcell');
                    const activeTdElement : HTMLElement[] = Array.from(tdElements).filter((tdElement: HTMLElement) => {
                        const formElement: HTMLElement = parentsUntil(tdElement, 'e-gridform') as HTMLElement ;
                        return formElement && formElement.querySelector('.e-griderror');
                    });
                    if (this.parent.options.frozenRows) {
                        if (parentsUntil(activeTdElement[0] as HTMLElement, 'e-gridheader')) {
                            isHeader = true;
                            element = activeTdElement[0] as HTMLElement;
                        }
                        else {
                            isHeader = false;
                        }
                    }
                    else if ((this.parent.options.enableVirtualization || this.parent.options.enableInfiniteScrolling) && parentsUntil(activeTdElement[0] as HTMLElement, 'e-gridheader')) {
                        element = activeTdElement[0] as HTMLElement;
                        isHeader = true;
                    }
                    else {
                        element = this.parent.getContent().querySelectorAll('[e-mappinguid=' + uid + ']')[0] as HTMLElement;
                        isHeader = false;
                    }
                }
                else {
                    const elements: NodeListOf<HTMLElement> = this.parent.getContent().querySelectorAll('[e-mappinguid=' + uid + ']');
                    for (let j: number = 0; j < elements.length; j++) {
                        if (!elements[parseInt(j.toString(), 10)].querySelector('.e-disabled')) {
                            element = elements[parseInt(j.toString(), 10)];
                        }
                    }
                }
            }
            if (isNullOrUndefined(element)) {
                const column: Column[] = this.parent.columnModel.filter((e: Column) => e.field.split(name).length > 1);
                if (!isNullOrUndefined(column) && column.length !== 0) {
                    name = column[0].field.replace(/[.]/g, '___');
                    element = this.parent.getContent().querySelectorAll('[e-mappinguid=' + uid + ']')[0] as HTMLElement || document.querySelectorAll('[e-mappinguid=' + uid + '_Dialog' + ']')[0] as HTMLElement;
                }
            }
            const isScroll: boolean = isHeader
                ? false : (gcontent.scrollHeight > gcontent.clientHeight || gcontent.scrollWidth > gcontent.clientWidth);
            const isInline: boolean = this.parent.options.editMode !== 'Dialog';
            const isAdaptive : boolean = this.parent.options.enableAdaptiveUI;
            const dialogWrapperText: string = isAdaptive ? '_adaptive_dialogEdit_wrapper' :   '_dialogEdit_wrapper';
            if (!element) { return; }
            const td: Element = closest(element, '.e-rowcell');
            const row: Element = closest(element, '.e-row');
            let isFHdr: boolean;
            const isFHdrLastRow: boolean = false;
            let validationForBottomRowPos: boolean;
            let isBatchModeLastRow: boolean = false;
            const viewPortRowCount: number = Math.round(this.parent.getContent().clientHeight / this.parent.getRowHeight()) - 1;
            let rows: Element[] = [].slice.call(this.parent.getContent().querySelectorAll('.e-row'));
            if (this.parent.options.enableVirtualization && this.parent.options.allowGrouping &&
                this.parent.options.groupCount > 0 && isAdd) {
                rows = [].slice.call(this.parent.getContent().querySelectorAll('tr'));
                rows.pop();
            }
            if (this.parent.options.editMode === 'Batch') {
                if (viewPortRowCount > 1 && rows.length >= viewPortRowCount
                    && parseInt(rows[rows.length - 1].getAttribute('aria-rowindex'), 10) - 1 === parseInt(row.getAttribute('aria-rowindex'), 10) - 1) {
                    isBatchModeLastRow = true;
                }
            }
            if (isInline) {
                // Cast row to HTMLElement before accessing offsetTop and offsetHeight
                const htmlRow: HTMLElement = row as HTMLElement;
                if (this.parent.options.frozenRows) {
                    // TODO: FrozenRows
                    // let fHeraderRows: HTMLCollection = this.parent.getFrozenColumns() ?
                    //     this.parent.getFrozenVirtualHeader().querySelector('tbody').children
                    //     : this.parent.getHeaderTable().querySelector('tbody').children;
                    // isFHdr = fHeraderRows.length > (parseInt(row.getAttribute('data-rowindex'), 10) || 0);
                    // isFHdrLastRow = isFHdr && parseInt(row.getAttribute('data-rowindex'), 10) === fHeraderRows.length - 1;
                }
                if (isFHdrLastRow || (viewPortRowCount > 1 &&
                    htmlRow.offsetTop + htmlRow.offsetHeight >= this.parent.getContent().clientHeight
                    && (this.parent.options.newRowPosition === 'Bottom' && isAdd || (!isNullOrUndefined(td)
                        && td.classList.contains('e-lastrowcell') && !row.classList.contains('e-addedrow')))) || isBatchModeLastRow) {
                    validationForBottomRowPos = true;
                }
                else if (isHeader && !isNullOrUndefined(row)) {
                    const rowIndexAttribute: string | null = row.getAttribute('aria-rowindex');
                    if (rowIndexAttribute !== null) {
                        const rowIndex: number = parseInt(rowIndexAttribute, 10) - 1;
                        if (rowIndex + 1 === this.parent.options.frozenRows) {
                            validationForBottomRowPos = true; // last frozen rows
                        }
                    }
                }
            }
            const table: Element = isInline ?
                ((isFHdr || isHeader) ? this.parent.getHeaderTable() : this.parent.getContentTable()) : document.querySelector('#' + this.parent.element.id + dialogWrapperText).querySelector('.e-dlg-content');
            const client: ClientRect = table.getBoundingClientRect();
            const left: number = isInline ?
                this.parent.element.getBoundingClientRect().left : client.left;
            const input: HTMLElement = closest(element, 'td') as HTMLElement;
            const inputClient: ClientRect = input ? input.getBoundingClientRect() : element.parentElement.getBoundingClientRect();
            const divUid: string = uid + '_Error';
            let div: HTMLElement;
            if (this.parent.options.hasTemplateInEditSettings || isNullOrUndefined(uid)) {
                div = this.parent.element.querySelector(`#${name}_Error`) || document.querySelector(`#${name}_Error`);
                if (this.parent.options.isRenderedFromTreeGrid) {
                    div = this.parent.element.querySelector(`#DataItem___${name}_Error`) as HTMLElement || document.querySelector(`#DataItem___${name}_Error`) as HTMLElement || div;
                }
            }
            else if ((this.parent.options.enableVirtualization || this.parent.options.frozenRows) && this.parent.options.showAddNewRow) {
                if (isHeader) {
                    div = this.parent.getHeaderContent().querySelectorAll('[e-mappinguid=' + divUid + ']')[0] as HTMLElement;
                }
                else {
                    div = this.parent.getContent().querySelectorAll('[e-mappinguid=' + divUid + ']')[0] as HTMLElement  || document.querySelectorAll('[e-mappinguid=' + divUid + ']')[0] as HTMLElement ;
                }

            }
            else {
                div = this.parent.getContent().querySelectorAll('[e-mappinguid=' + divUid + ']')[0] as HTMLElement || document.querySelectorAll('[e-mappinguid=' + divUid + ']')[0] as HTMLElement;
            }
            if (isNullOrUndefined(div)) {
                return;
            }
            div.style.top =
            (((isFHdr || isHeader) ? inputClient.top + inputClient.height : inputClient.bottom - client.top) + table.scrollTop + 9) + 'px';
            div.style.left =
            (inputClient.left - left + table.scrollLeft + inputClient.width / 2) + 'px';
            div.style.maxWidth =  'auto';
            if (isInline && client.left < left) {
                div.style.left = parseInt(div.style.left, 10) - client.left + left + 'px';
            }
            let arrow: Element;
            if (validationForBottomRowPos) {
                arrow = div.querySelector('.e-tip-bottom');
            } else {
                arrow = div.querySelector('.e-tip-top');
            }
            if ((this.parent.options.frozenColumns || this.parent.options.frozenRows) && this.parent.options.editMode !== 'Dialog') {
                const getEditCell: HTMLElement = this.parent.options.editMode === 'Normal' ?
                    closest(element, '.e-editcell') as HTMLElement : closest(element, '.e-table') as HTMLElement;
                getEditCell.style.position = 'relative';
                div.style.position = 'absolute';
            }
            div.style.display = 'block';
            const errorLabel: HTMLElement | null  = div.querySelector('.e-error') as HTMLElement;
            let tempLabelElement: HTMLElement | null = null;
            if (!isNullOrUndefined(errorLabel)) { 
                errorLabel.innerText = message; 
            }
            else {
                tempLabelElement = document.createElement('label');
                tempLabelElement.className = 'e-error';
                tempLabelElement.innerText = message;
                const tipContent: HTMLElement | null  = div.querySelector('.e-tip-content');
                if(!isNullOrUndefined(tipContent)) {
                    tipContent.appendChild(tempLabelElement);
                }
            }
            if (!validationForBottomRowPos && isInline &&
                gcontent.getBoundingClientRect().bottom < inputClient.bottom + inputClient.height) {
                gcontent.scrollTop = gcontent.scrollTop + div.offsetHeight + arrow.scrollHeight;
            }
            const lineHeight: number = parseInt(
                document.defaultView.getComputedStyle(div, null).getPropertyValue('font-size'), 10
            );
            const labelElement: HTMLElement | null = div.querySelector('label') as HTMLElement;
            if ((div.getBoundingClientRect().width < inputClient.width &&
                labelElement && labelElement.getBoundingClientRect().height / (lineHeight * 1.2) >= 2) && (this.parent.options.editMode !== 'Dialog')) {
                div.style.width = inputClient.width - 4 + 'px';
            }
            if ((this.parent.options.frozenColumns || this.parent.options.frozenRows)
                && (this.parent.options.editMode === 'Normal' || this.parent.options.editMode === 'Batch')) {
                div.style.left = input.offsetLeft + (input.offsetWidth / 2 - div.offsetWidth / 2) + 'px';
            } else {
                div.style.left = (parseInt(div.style.left, 10) - div.offsetWidth / 2) + 'px';
            }
            if (isInline && !isScroll && !this.parent.options.allowPaging || this.parent.options.frozenColumns
                 || this.parent.options.frozenRows) {
                if (!this.parent.options.showAddNewRow && !this.parent.options.enableAutoFill && !this.parent.options.enableVirtualMaskRow
                    && !this.parent.options.enableVirtualization)
                {
                    gcontent.style.position = 'static';
                }
                const pos: OffsetPosition = calculateRelativeBasedPosition(input, div);
                div.style.top = pos.top + inputClient.height + 9 + 'px';
            } else if (gcontent.style.position !== '') {
                gcontent.style.position = '';
            }
            if (validationForBottomRowPos) {
                if (isScroll && !this.parent.options.frozenColumns && this.parent.options.height !== 'auto' && !this.parent.options.frozenRows
                    && !this.parent.options.enableVirtualization)
                {
                    const scrollWidth: number = gcontent.scrollWidth > gcontent.offsetWidth ? getScrollBarWidth() : 0;
                    const gHeight: number = this.parent.options.height.toString().indexOf('%') === -1 ? parseInt(this.parent.options.height, 10) : gcontent.offsetHeight;
                    div.style.bottom = (gHeight - gcontent.querySelector('table').offsetHeight
                        - scrollWidth) + inputClient.height + 9 + 'px';
                } else {
                    div.style.bottom = inputClient.height + 9 + 'px';
                }
                //TODO: NEW LINES ADDED SHOULD CHECK
                // if (rows.length < viewPortRowCount && this.parent.editSettings.newRowPosition === 'Bottom' && (this.editModule.args
                //     && this.editModule.args.requestType === 'add')) {
                //     let rowsCount: number = this.parent.frozenRows ? this.parent.frozenRows + (rows.length - 1) : rows.length - 1;
                //     let rowsHeight: number = rowsCount * this.parent.getRowHeight();
                //     let position: number = this.parent.getContent().clientHeight - rowsHeight;
                //     div.style.bottom = position + 9 + 'px';
                // }
                div.style.top = null;
            }
            // div.style.display = 'none';
            if (input && this.parent.options.enableRtl && !this.parent.options.enableVirtualization) {
                const inputElement : HTMLElement = input.querySelector('.e-input') as HTMLElement;
                const inputRight : number = inputElement.getBoundingClientRect().right;
                const elemRight : number = div.getBoundingClientRect().right;
                if (elemRight > inputRight) {
                    const offSet : number = elemRight - inputRight;
                    div.style.left = (div.offsetLeft - offSet) + 'px';
                }
            }
            arrowPosition = validationForBottomRowPos ? 'bottom' : 'top';
            if (name.includes('___') && isNullOrUndefined(uid)) {
                name = name.replace('___', '.');
            }

            const columnUid: string = (() => {
                if (!isNullOrUndefined(uid)) {
                    return uid;
                }
                const col: Column = this.parent.getColumnByField(name);

                if (col !== null && col !== undefined && col.uid !== null && col.uid !== undefined) {
                    return col.uid;
                }

                if (this.parent.options.isRenderedFromTreeGrid) {
                    const treeCol: Column = this.parent.getColumnByField('DataItem.' + name);
                    if (treeCol !== null && treeCol !== undefined && treeCol.uid !== null && treeCol.uid !== undefined) {
                        return treeCol.uid;
                    }
                }
                return uid;
            })();

            toolTipPos[`${columnUid}`] = `top: ${div.style.top}; bottom: ${div.style.bottom}; left: ${div.style.left}; 
            max-width: ${div.style.maxWidth}; width: ${div.style.width}; text-align: center; position: ${div.style.position};`;
            if(!isNullOrUndefined(tempLabelElement)) {
                tempLabelElement.remove();
            }
        }
        this.parent.dotNetRef.invokeMethodAsync('ShowValidationPopup', toolTipPos, arrowPosition);
    }
}
