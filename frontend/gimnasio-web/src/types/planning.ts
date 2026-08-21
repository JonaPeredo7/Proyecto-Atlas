export interface Goal{id:string;title:string;category:string;baselineValue:number|null;targetValue:number|null;unit:string|null;startDate:string;targetDate:string|null;status:string;rationale:string|null;version:number;metricDefinitionId:string|null;metricName:string|null;latestValue:number|null;latestDate:string|null;progressPercent:number|null}
export interface Cycle{id:string;name:string;startDate:string;endDate:string;focus:string;plannedSessionsPerWeek:number;status:string;notes:string|null;version:number;expectedSessions:number;completedSessions:number;adherencePercent:number|null}
export interface PlanChange{id:string;entityType:string;entityId:string;version:number;reason:string;summary:string;changedAt:string}
export interface MetricOption{id:string;name:string;unit:string;direction:string}
export interface PlanningOverview{goals:Goal[];cycles:Cycle[];changes:PlanChange[];metrics:MetricOption[]}
