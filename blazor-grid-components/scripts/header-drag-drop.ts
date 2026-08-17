import { Draggable, Droppable, DropEventArgs, isNullOrUndefined, createElement, MouseEventArgs } from '@syncfusion/ej2-base';
import { BlazorDragEventArgs, remove, closest as getClosest, classList, EventHandler } from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { parentsUntil } from './util';
import { Column } from './interfaces';

// eslint-disable-next-line valid-jsdoc, jsdoc/require-param, jsdoc/require-returns
/**
 * Constructor for the HeaderDragDrop class.
 *
 */
export class HeaderDragDrop {

    private parent: SfGrid;
    private column: Column;
    private draggable: Draggable;
    private droppable: Droppable;
    /**
     * Constructor for HeaderDragDrop class.
     *
     * @param {SfGrid} parent - The parent SfGrid instance.
     */
    constructor(parent: SfGrid) {
        this.parent = parent;

        if (this.parent.options.allowGrouping || this.parent.options.allowReordering) {
            this.initializeHeaderDrag();
            this.initializeHeaderDrop();
        }
    }

    /**
     * Initializes header drag functionality.
     *
     * @public
     * @returns {void}
     */
    public initializeHeaderDrag(): void {
        const gObj: SfGrid = this.parent;
        if (!(this.parent.options.allowReordering || (this.parent.options.allowGrouping && this.parent.options.showDropArea))) {
            return;
        }
        const headerRows: Element[] = [].slice.call(gObj.getHeaderContent().querySelectorAll('.e-columnheader'));
        for (let i: number = 0, len: number = headerRows.length; i < len; i++) {
            this.draggable = new Draggable(headerRows[parseInt(i.toString(), 10)] as HTMLElement, {
                dragTarget: '.e-headercell',
                distance: 5,
                helper: this.helper,
                dragStart: this.dragStart,
                drag: this.drag,
                dragStop: this.dragStop,
                abort: '.e-rhandler',
                isPreventSelect: false
            });
        }
    }
    /**
     * Initializes header drop functionality.
     *
     * @private
     * @returns {void}
     */
    public initializeHeaderDrop(): void {
        const gObj: SfGrid = this.parent;
        this.droppable = new Droppable(gObj.getHeaderContent() as HTMLElement, {
            accept: '.e-dragclone',
            drop: this.drop as (e: DropEventArgs) => void
        });
    }
    // eslint-disable-next-line valid-jsdoc, jsdoc/require-param
    /**
     * Function handler for drag start event.
     *
     * @returns {void}
     */
    private dragStart: Function = (e: { target: HTMLElement, event: MouseEvent } & BlazorDragEventArgs) => {
        const gObj: SfGrid = this.parent;
        document.body.classList.add('e-prevent-select');
        const popup: HTMLElement = (gObj.element.querySelector('.e-gridpopup') as HTMLElement);
        if (popup) {
            popup.style.display = 'none';
        }
        this.parent.reorderModule.dragStart({ target: e.target, column: this.column, event: e.event });

        this.parent.groupModule.columnDragStart({ target: e.target, column: this.column, event: e.event});

        e.bindEvents(e.dragElement);
    }
    private drag: Function = (e: { target: HTMLElement, event: MouseEventArgs }): void => {
        const gObj: SfGrid = this.parent;
        const target: Element = e.target;
        if (target) {
            const closest: Element = getClosest(target, '.e-grid');
            const contentElement: Element = getClosest(target, '.e-gridcontent');
            const pagerElement: Element = getClosest(target, '.e-gridpager');
            const cloneElement: HTMLElement = this.parent.element.querySelector('.e-cloneproperties') as HTMLElement;
            if (!closest || closest.getAttribute('id') !== gObj.element.getAttribute('id') || (contentElement && closest.getAttribute('id') === getClosest(contentElement, '.e-grid').getAttribute('id')) || pagerElement) {
                classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
                if (gObj.options.allowReordering) {
                    const upArrowElement : HTMLElement | null = gObj.options.enableColumnVirtualization ? gObj.element.querySelector('.e-reorderuparrow-virtual') : gObj.element.querySelector('.e-reorderuparrow');
                    const downArrowElement : HTMLElement | null = gObj.options.enableColumnVirtualization ? gObj.element.querySelector('.e-reorderdownarrow-virtual') : gObj.element.querySelector('.e-reorderdownarrow');
                    if (upArrowElement){
                        (upArrowElement as HTMLElement).style.display = 'none';
                    }
                    if (downArrowElement){
                        (downArrowElement as HTMLElement).style.display = 'none';
                    }
                }
                if (!gObj.options.groupReordering) {
                    return;
                }
            }
            if (gObj.options.allowReordering) {
                this.parent.reorderModule.drag({ target: e.target, column: this.column, event: e.event });
            }
            if (gObj.options.allowGrouping) {
                this.parent.groupModule.columnDrag({ target: e.target });
            }
        }
    }
    private dragStop: Function = (e: { target: HTMLElement, event: MouseEventArgs, helper: Element }) => {
        const gObj: SfGrid = this.parent;
        document.body.classList.remove('e-prevent-select');
        let cancel: boolean;
        const popup: HTMLElement = (gObj.element.querySelector('.e-gridpopup') as HTMLElement);
        if (popup) {
            popup.style.display = 'none';
        }
        if ((!parentsUntil(e.target, 'e-headercell') && !parentsUntil(e.target, 'e-groupdroparea')) ||
            (!gObj.options.allowReordering && parentsUntil(e.target, 'e-headercell')) ||
            (!e.helper.getAttribute('e-mappinguid') && parentsUntil(e.target, 'e-groupdroparea'))) {
            remove(e.helper);
            cancel = true;
            if (gObj.options.allowGrouping && this.parent.groupModule) {
                EventHandler.remove(window as any, 'touchmove', this.parent.groupModule.preventTouchOnWindow);
            }
        }
        if (gObj.options.allowReordering) {
            this.parent.reorderModule.dragStop({ target: e.target, event: e.event, column: this.column, cancel: cancel });
        }
    }
    private drop: Function = (e: DropEventArgs) => {
        const gObj: SfGrid = this.parent;
        const closest: Element = getClosest(e.target, '.e-grid');
        remove(e.droppedElement);
        if (closest && closest.getAttribute('id') !== gObj.element.getAttribute('id') ||
            !(gObj.options.allowReordering || gObj.options.allowGrouping)) {
            return;
        }
        if (gObj.options.allowReordering) {
            this.parent.reorderModule.headerDrop({ target: e.target });
        }
        if (gObj.options.allowGrouping && gObj.options.showDropArea) {
            this.parent.groupModule.columnDrop({
                target: e.target, droppedElement: e.droppedElement
            });
        }
        //gObj.notify(events.headerDrop, { target: e.target, uid: uid, droppedElement: e.droppedElement });

    }

    private helper: Function = (e: { sender: MouseEvent, element: Element, currentTargetElement: HTMLElement }) => {
        const gObj: SfGrid = this.parent;
        let target: Element = (e.sender.target as Element);
        if (gObj.options.isFixedColumnPresent && e.currentTargetElement && e.sender.target) {
            const currentTargetHeader: Element = parentsUntil(e.currentTargetElement, 'e-headercell');
            const senderTargetHeader: Element = parentsUntil((e.sender.target as Element), 'e-headercell');
            if (currentTargetHeader && senderTargetHeader) {
                target = e.currentTargetElement;
            }
        }
        const closest: HTMLElement = getClosest(target, '.e-headercell:not(.e-stackedHeaderCell)') as HTMLElement;
        if (closest) {
            const dropElement: Element = closest.querySelector('.e-headercelldiv') || closest.querySelector('.e-stackedheadercelldiv');
            const uID: string = dropElement.getAttribute('e-mappinguid');
            const column: Column = gObj.getColumnByUid(uID);
            if (!isNullOrUndefined(column) && !column.allowGrouping && !column.allowReordering) {
                return false;
            }
        }
        const parentEle: HTMLElement = parentsUntil(target, 'e-headercell') as HTMLElement;
        if (gObj.getContent().classList.contains('e-freezeline-moving') || !(gObj.options.allowReordering || gObj.options.allowGrouping) || (!isNullOrUndefined(parentEle)
            && parentEle.querySelectorAll('.e-checkselectall').length > 0)) {
            return false;
        }
        const visualElement: HTMLElement = createElement('div', { className: 'e-cloneproperties e-dragclone e-headerclone' });
        const element: HTMLElement = target.classList.contains('e-headercell') ? target as HTMLElement : parentEle;
        if (!element || (!gObj.options.allowReordering && element.classList.contains('e-stackedheadercell'))) {
            return false;
        }
        const height: number = element.offsetHeight;
        const headercelldiv: Element = element.querySelector('.e-headercelldiv') || element.querySelector('.e-stackedheadercelldiv');
        let col: Column;
        if (headercelldiv) {
            if (element.querySelector('.e-stackedheadercelldiv')) {
                col = gObj.getStackedHeaderColumnByHeaderText(
                    (headercelldiv as HTMLElement).innerText.trim(), <Column[]>gObj.options.columns);
            } else {
                col = gObj.getColumnByUid(headercelldiv.getAttribute('e-mappinguid'));
            }
            this.column = col;
            const stackedColumns: Column[] = this.column.columns;
            const fixedColumnsOrder: Column[] = this.parent.reorderModule.processStackedColumns(this.parent.options.columns);
            const indexOftheElement: number = parseInt(element.getAttribute('aria-colindex'), 10) - 1;
            const isStackedChildAllLocked: boolean = !isNullOrUndefined(stackedColumns) && stackedColumns.length > 0 ?
                this.parent.reorderModule.isAnyColumnFixed(fixedColumnsOrder[parseInt(indexOftheElement.toString(), 10)]) : false;
            if (this.column.fixedColumn || isStackedChildAllLocked) {
                return false;
            }
            visualElement.setAttribute('e-mappinguid', headercelldiv.getAttribute('e-mappinguid'));
        }
        visualElement.innerText = headercelldiv ? isNullOrUndefined(col.headerText) ?
            col.field : col.headerText : element.innerText;
        visualElement.style.width = element.offsetWidth + 'px';
        visualElement.style.height = element.offsetHeight + 'px';
        visualElement.style.lineHeight = (height - 6).toString() + 'px';
        gObj.element.appendChild(visualElement);
        return visualElement;
    }

    public destroy(): void {
        if (!isNullOrUndefined(this.draggable)) {
            this.draggable.destroy();
        }
        if (!isNullOrUndefined(this.droppable)) {
            this.droppable.destroy();
        }
    }
}
