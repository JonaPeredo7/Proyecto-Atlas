export interface LearningOption { id:string; name:string; detail:string }
export type LearningOutcome = 'helpful'|'neutral'|'not-helpful'|'inconclusive'
export interface LearningEntry {
  id:string; date:string; title:string; category:string; observation:string; interpretation:string|null;
  nextAction:string|null; reviewDueOn:string|null; confidence:'low'|'medium'|'high'; status:'open'|'applied'|'archived';
  reviewedOn:string|null; followUpOutcome:LearningOutcome|null; followUpObservation:string|null;
  trainingSessionId:string|null; trainingSessionName:string|null; personalGoalId:string|null;
  personalGoalName:string|null; trainingCycleId:string|null; trainingCycleName:string|null; version:number
}
export interface LearningOverview {
  entries:LearningEntry[]; sessions:LearningOption[]; goals:LearningOption[]; cycles:LearningOption[];
  openActions:number; reviewedActions:number; entriesLast30Days:number
}
