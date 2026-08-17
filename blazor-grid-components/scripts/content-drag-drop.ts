import { Droppable, DropEventArgs, remove, isNullOrUndefined} from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { parentsUntil } from './util';
/**
 * ColumnDrop Handling
 */
export class ContentDragDrop {
    /**
     * Handles the drop event for columns.
     *
     * @param {DropEventArgs} e - The event arguments for the drop event.
     * @returns {void}
     */
    private drop: Function = (e: DropEventArgs) => {
        const targetElementGrid : HTMLElement = parentsUntil(e.target, 'e-grid') as HTMLElement;
        const cloneElementGrid: HTMLElement = e.droppedElement.parentElement;
        const elementsNullCaseCheck: boolean = !isNullOrUndefined(targetElementGrid) && !isNullOrUndefined(cloneElementGrid);
        if (this.parent.options.allowRowDragAndDrop && this.parent.options.rowDropTarget) {
            if (elementsNullCaseCheck && targetElementGrid.id !== cloneElementGrid.id && this.parent.options.groupCount === 0) {
                this.parent.rowDragAndDropModule.columnDrop({
                    target: e.target as HTMLTableRowElement,
                    droppedElement: e.droppedElement,
                    mouseEvent: e.event as MouseEvent
                });
            }
        }
        else{
            this.parent.groupModule.columnDrop({
                target: e.target, droppedElement: e.droppedElement
            });
        }
        remove(e.droppedElement);
    }

    private parent: SfGrid;
    constructor(parent: SfGrid) {
        this.parent = parent;
        if (this.parent.options.allowGrouping) {
            this.initializeContentDrop();
        }
    }

    public initializeContentDrop(): void {
        const gObj: SfGrid = this.parent;
        new Droppable(gObj.getContent() as HTMLElement, {
            accept: '.e-dragclone',
            drop: this.drop as (e: DropEventArgs) => void
        });
    }
}
