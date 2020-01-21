import { EventEmitter } from '../../../stencil.core';
import 'dragscroll';
import { Activity, ActivityDefinition, Workflow, WorkflowFormatDescriptor } from "../../../models";
import "../../../drivers";
import '../../../plugins/console-activities';
import '../../../plugins/control-flow-activities';
import '../../../plugins/email-activities';
import '../../../plugins/http-activities';
import '../../../plugins/mass-transit-activities';
import '../../../plugins/primitives-activities';
import '../../../plugins/timer-activities';
export declare class DesignerHost {
    activityEditor: HTMLWfActivityEditorElement;
    activityPicker: HTMLWfActivityPickerElement;
    designer: HTMLWfDesignerElement;
    importExport: HTMLWfImportExportElement;
    el: HTMLElement;
    activityDefinitions: Array<ActivityDefinition>;
    workflow: Workflow;
    canvasHeight: string;
    activityDefinitionsData: string;
    workflowData: string;
    readonly: boolean;
    pluginsData: string;
    newWorkflow(): Promise<void>;
    getWorkflow(): Promise<any>;
    showActivityPicker(): Promise<void>;
    export(formatDescriptor: WorkflowFormatDescriptor): Promise<void>;
    import(): Promise<void>;
    onActivityPicked(e: CustomEvent<ActivityDefinition>): Promise<void>;
    onEditActivity(e: CustomEvent<Activity>): Promise<void>;
    onAddActivity(): Promise<void>;
    onUpdateActivity(e: CustomEvent<Activity>): Promise<void>;
    onExportWorkflow(e: CustomEvent<WorkflowFormatDescriptor>): Promise<void>;
    onImportWorkflow(e: CustomEvent<Workflow>): Promise<void>;
    workflowChanged: EventEmitter;
    private loadActivityDefinitions;
    private onWorkflowChanged;
    private initActivityDefinitions;
    private initFieldDrivers;
    private initWorkflow;
    componentWillLoad(): void;
    componentDidLoad(): void;
    render(): any;
}
