import { EventHandler, MouseEventArgs, Draggable, Droppable, DropEventArgs } from '@syncfusion/ej2-base';
import { createElement, remove, classList, BlazorDragEventArgs } from '@syncfusion/ej2-base';
import { isNullOrUndefined } from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { Column } from './interfaces';
import { parentsUntil } from './util';

// eslint-disable-next-line valid-jsdoc, jsdoc/require-param
/**
 *
 * The `Group` module is used to handle group action.
 */
export class Group {
    //Internal variables
    private element: HTMLElement;
    private column: Column;

    private visualElement: HTMLElement = createElement('div', {
        className: 'e-cloneproperties e-dragclone e-gdclone',
        styles: 'line-height:23px', attrs: { action: 'grouping' }
    });

    /**
     *
     * Helper function used for handling group action.
     *
     * @param {MouseEvent} e - The MouseEvent object.
     * @param {HTMLElement} e.sender - The sender element of the event.
     * @returns {void}
     */
    private helper: Function = (e: { sender: MouseEvent }) => {
        const gObj: SfGrid = this.parent;
        const target: Element = (e.sender.target as Element);
        const element: HTMLElement = target.classList.contains('e-groupheadercell') ? target as HTMLElement :
            parentsUntil(target, 'e-groupheadercell') as HTMLElement;
        if (!element || (!target.classList.contains('e-drag') && this.parent.options.groupReordering)) {
            return false;
        }
        const groupingContainer: HTMLElement | null = (element.closest('.e-groupdroparea') as HTMLElement | null) || (element.parentElement as HTMLElement | null);
        if (!groupingContainer) { return false; }

        const groupingFieldIndex: number = Array.prototype.indexOf.call(groupingContainer.querySelectorAll('.e-groupheadercell'), element);
        if (groupingFieldIndex === -1) { return false; }

        const initGroupingFields: string[] | undefined = gObj.options.initGroupingField;
        const groupingField: string | undefined = initGroupingFields[Number(groupingFieldIndex)];

        this.column = gObj.getColumnByField(groupingField);
        if (!this.column) { return false; }

        this.visualElement.textContent = element.textContent;
        this.visualElement.style.width = element.offsetWidth + 2 + 'px';
        this.visualElement.style.height = element.offsetHeight + 2 + 'px';
        this.visualElement.setAttribute('e-mappinguid', this.column.uid);
        gObj.element.appendChild(this.visualElement);
        return this.visualElement;
    }
    private dragStart: Function = (e: BlazorDragEventArgs): void => {
        this.parent.element.classList.add('e-ungroupdrag');
        document.body.classList.add('e-prevent-select');
        e.bindEvents(e.dragElement);
    }
    private drag: Function = (e: { target: HTMLElement, event: MouseEventArgs }): void => {
        // if (this.groupSettings.allowReordering) {
        //     this.animateDropper(e);
        // }
        const target: Element = e.target;
        const cloneElement: HTMLElement = this.parent.element.querySelector('.e-cloneproperties') as HTMLElement;
        // this.parent.trigger(events.columnDrag, { target: target, draggableType: 'headercell', column: this.column });
        if (!this.parent.options.groupReordering) {
            classList(cloneElement, ['e-defaultcur'], ['e-notallowedcur']);
            if (!(parentsUntil(target as Element, 'e-gridcontent') || parentsUntil(target as Element, 'e-headercell'))) {
                classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
            }
        }
    }
    private dragStop: Function = (e: { target: HTMLElement, event: MouseEventArgs, helper: Element }) => {
        document.body.classList.remove('e-prevent-select');
        this.parent.element.classList.remove('e-ungroupdrag');
        const preventDrop: boolean = !(parentsUntil(e.target, 'e-gridcontent') || parentsUntil(e.target, 'e-gridheader'));
        // if (this.groupSettings.allowReordering && preventDrop) {     //TODO: reordering
        //     remove(e.helper);
        //     if (parentsUntil(e.target, 'e-groupdroparea')) {
        //         this.rearrangeGroup(e);
        //     } else if (!(parentsUntil(e.target, 'e-grid'))) {
        //         let field: string = this.parent.getColumnByUid(e.helper.getAttribute('e-mappinguid')).field;
        //         if (this.groupSettings.columns.indexOf(field) !== -1) {
        //             this.ungroupColumn(field);
        //         }
        //     }
        //     return;
        // } else
        if (preventDrop) {
            remove(e.helper);
            return;
        }
    }
    //TODO: reordering
    // private animateDropper: Function = (e: { target: HTMLElement, event: MouseEventArgs, helper: Element }) => {
    //     let uid: string = this.parent.element.querySelector('.e-cloneproperties').getAttribute('e-mappinguid');
    //     let dragField: string = this.parent.getColumnByUid(uid).field;
    //     let parent: Element = parentsUntil(e.target, 'e-groupdroparea');
    //     let dropTarget: Element = parentsUntil(e.target, 'e-group-animator');
    //     // tslint:disable-next-line
    //     let grouped: string[] = [].slice.call(this.element.querySelectorAll('.e-groupheadercell'))
    //         .map((e: Element) => e.querySelector('div').getAttribute('ej-mappingname'));
    //     let cols: string[] = JSON.parse(JSON.stringify(grouped));
    //     if (dropTarget || parent) {
    //         if (dropTarget) {
    //             let dropField: string = dropTarget.querySelector('div[ej-mappingname]').getAttribute('ej-mappingname');
    //             let dropIndex: number = +(dropTarget.getAttribute('index'));
    //             if (dropField !== dragField) {
    //                 let dragIndex: number = cols.indexOf(dragField);
    //                 if (dragIndex !== -1) {
    //                     cols.splice(dragIndex, 1);
    //                 }
    //                 let flag: boolean = dropIndex !== -1 && dragIndex === dropIndex;
    //                 cols.splice(dropIndex + (flag ? 1 : 0), 0, dragField);
    //             }
    //         } else if (parent && cols.indexOf(dragField) === -1) {
    //             cols.push(dragField);
    //         }
    //         this.element.innerHTML = '';
    //         if (cols.length && !this.element.classList.contains('e-grouped')) {
    //             this.element.classList.add('e-grouped');
    //         }
    //         this.reorderingColumns = cols;
    //         for (let c: number = 0; c < cols.length; c++) {
    //             this.addColToGroupDrop(cols[c]);
    //         }
    //     } else {
    //         this.addLabel();
    //         this.removeColFromGroupDrop(dragField);
    //     }

    // }

    // private rearrangeGroup(e: { target: HTMLElement, event: MouseEventArgs, helper: Element }): void {
    //     this.sortRequired = false;
    //     this.updateModel();
    // }

    public preventTouchOnWindow: Function = (e: TouchEvent) => {
        e.preventDefault();
    }

    private drop: Function = (e: DropEventArgs) => {
        const gObj: SfGrid = this.parent;
        const column: Column = gObj.getColumnByUid(e.droppedElement.getAttribute('e-mappinguid'));
        gObj.element.querySelector('.e-groupdroparea').classList.remove('e-hover');
        remove(e.droppedElement);
        if (gObj.options.allowGrouping) {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            EventHandler.remove(window as any, 'touchmove', this.preventTouchOnWindow);
        }
        this.parent.element.querySelector('.e-groupdroparea').removeAttribute('aria-dropeffect');
        this.parent.element.querySelector('[aria-grabbed=true]').setAttribute('aria-grabbed', 'false');
        if (isNullOrUndefined(column) || column.allowGrouping === false
            || parentsUntil(gObj.getColumnHeaderByUid(column.uid), 'e-grid').getAttribute('id') !==
             gObj.element.getAttribute('id')) {
            return;
        }
        gObj.dotNetRef.invokeMethodAsync('GroupColumn', column.field, 'Group');
    }
    //Module declarations
    private parent: SfGrid;

    /**
     *
     * Constructor for Grid group module
     *
     * @param {SfGrid} parent - Specifies that parent is an optional parameter of type SfGrid
     * @hidden
     */
    constructor(parent?: SfGrid) {
        this.parent = parent;

        if (this.parent.options.allowGrouping && this.parent.options.showDropArea) {
            this.initDragAndDrop();
        }
    }

    public columnDrag(e: { target: Element }): void {
        if (this.parent.options.groupReordering) {
            // this.animateDropper(e);
        }
        const gObj: SfGrid = this.parent;
        const cloneElement: HTMLElement = gObj.element.querySelector('.e-cloneproperties') as HTMLElement;
        const column: Column = gObj.getColumnByUid(cloneElement.getAttribute('e-mappinguid'));
        if (parentsUntil(e.target as Element, 'e-groupdroparea')) {
            classList(cloneElement, ['e-defaultcur'], ['e-notallowedcur']);
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        EventHandler.add(window as any, 'touchmove', this.preventTouchOnWindow);
        if (!parentsUntil(e.target as Element, 'e-groupdroparea') &&
            !(gObj.options.allowReordering && parentsUntil(e.target as Element, 'e-headercell')) || (!column.allowGrouping && parentsUntil(e.target as Element, 'e-groupdroparea'))) {
            classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
        }
        if (gObj.options.showDropArea) {
            if (e.target.classList.contains('e-groupdroparea')) {
                gObj.element.querySelector('.e-groupdroparea').classList.add('e-hover');
            }
            else {
                gObj.element.querySelector('.e-groupdroparea').classList.remove('e-hover');
            }
        }
    }

    public columnDragStart(e: { target: Element, column: Column, event: MouseEvent }): void {
        if (e.target.classList.contains('e-stackedheadercell')) {
            return;
        }
        const dropArea: HTMLElement = <HTMLElement>this.parent.element.querySelector('.e-groupdroparea');
        if (dropArea) {
            dropArea.setAttribute('aria-dropeffect', 'copy');
        }
        let element: Element = e.target.classList.contains('e-headercell') ? e.target : parentsUntil(e.target as Element, 'e-headercell');
        if (isNullOrUndefined(element)) {
            element = (e.event.target as Element).classList.contains('e-headercell') ? e.event.target as Element : parentsUntil(e.event.target as Element, 'e-headercell');
        }
        element.setAttribute('aria-grabbed', 'true');
    }

    public columnDrop(e: { target: Element, droppedElement: HTMLElement }): void {
        const gObj: SfGrid = this.parent;
        if (e.droppedElement.getAttribute('action') === 'grouping') {
            const column: Column = gObj.getColumnByUid(e.droppedElement.getAttribute('e-mappinguid'));
            if (isNullOrUndefined(column) || column.allowGrouping === false
             || parentsUntil(gObj.getColumnHeaderByUid(column.uid), 'e-grid').getAttribute('id') !==
                 gObj.element.getAttribute('id')) {
                return;
            }
            gObj.dotNetRef.invokeMethodAsync('GroupColumn', column.field, 'Ungroup');
        }
    }

    // private keyPressHandler(e: KeyboardEventArgs): void {
    //     let gObj: SfGrid = this.parent;
    //     if (e.action !== 'ctrlSpace' && (!this.groupSettings.columns.length ||
    //         ['altDownArrow', 'altUpArrow', 'ctrlDownArrow', 'ctrlUpArrow', 'enter'].indexOf(e.action) === -1)) {
    //         return;
    //     }
    //     e.preventDefault();
    //     switch (e.action) {
    //         case 'altDownArrow':
    //         case 'altUpArrow':
    //             let selected: number[] = gObj.allowSelection ? gObj.getSelectedRowIndexes() : [];
    //             if (selected.length) {
    //                 let rows: HTMLCollection = gObj.getContentTable().querySelector('tbody').children;
    //                 let dataRow: HTMLTableRowElement = gObj.getDataRows()[selected[selected.length - 1]] as HTMLTableRowElement;
    //                 let grpRow: Element;
    //                 for (let i: number = dataRow.rowIndex; i >= 0; i--) {
    //                     if (!rows[i].classList.contains('e-row') && !rows[i].classList.contains('e-detailrow')) {
    //                         grpRow = rows[i];
    //                         break;
    //                     }
    //                 }
    //                 this.expandCollapseRows(grpRow.querySelector(e.action === 'altUpArrow' ?
    //                     '.e-recordplusexpand' : '.e-recordpluscollapse'));
    //             }
    //             break;
    //         case 'ctrlDownArrow':
    //             this.expandAll();
    //             break;
    //         case 'ctrlUpArrow':
    //             this.collapseAll();
    //             break;
    //         case 'enter':
    //             if (this.parent.isEdit || (closest(e.target as Element, '#' + this.parent.element.id + '_searchbar') !== null)) { return; }
    //             let element: HTMLElement = this.focus.getFocusedElement();
    //             let row: Element = element ? element.parentElement.querySelector('[class^="e-record"]') : null;
    //             if (!row) { break; }
    //             this.expandCollapseRows(row);
    //             break;
    //         case 'ctrlSpace':
    //             let elem: HTMLElement = gObj.focusModule.currentInfo.element;
    //             if (elem && elem.classList.contains('e-headercell')) {
    //                 let column: Column = gObj.getColumnByUid(elem.firstElementChild.getAttribute('e-mappinguid'));
    //                 column.field && gObj.groupSettings.columns.indexOf(column.field) < 0 ?
    //                     this.groupColumn(column.field) : this.ungroupColumn(column.field);
    //             }
    //             break;

    //     }
    // }

    public initDragAndDrop(): void {
        this.initializeGHeaderDrop();
        this.initializeGHeaderDrag();
    }

    public initializeGHeaderDrag(): void {
        const ele: Element = this.parent.element.querySelector('.e-groupdroparea');
        if (!ele) { return; }
        new Draggable(ele as HTMLElement, {
            dragTarget: this.parent.options.groupReordering ? '.e-drag' : '.e-groupheadercell',
            distance: this.parent.options.groupReordering ? -10 : 5,
            helper: this.helper,
            dragStart: this.dragStart,
            drag: this.drag,
            dragStop: this.dragStop,
            isPreventSelect: false
        });
    }

    public initializeGHeaderDrop(): void {
        const ele: Element = this.parent.element.querySelector('.e-groupdroparea');
        if (!ele) { return; }
        new Droppable(ele as HTMLElement, {
            accept: '.e-dragclone',
            drop: this.drop as (e: DropEventArgs) => void
        });
    }

    /**
     *
     * For internal use only - Get the module name.
     *
     * @private
     * @returns {string} The name of the module.
     */
    protected getModuleName(): string {
        return 'group';
    }

    private getGHeaderCell(field: string): Element {
        if (this.element && this.element.querySelector('[ej-mappingname="' + field + '"]')) {
            return this.element.querySelector('[ej-mappingname="' + field + '"]').parentElement;
        }
        return null;
    }
}
