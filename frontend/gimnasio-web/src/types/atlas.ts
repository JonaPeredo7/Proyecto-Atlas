export interface AtlasProfile {
  id: string
  displayName: string
  heightCm: number | null
  referenceWeightKg: number | null
  primaryGoal: string | null
  targetDate: string | null
  dominantHand: string | null
  dominantLeg: string | null
  affectedKnee: string | null
}

export interface ProfileFact {
  id: string
  category: string
  label: string
  value: string | null
  status: string
  statusLabel: string
  sourceTitle: string
  notes: string | null
}

export interface DailyCheckIn {
  id: string
  date: string
  sleepMinutes: number | null
  sleepQuality: number
  energy: number
  fatigue: number
  stress: number
  painLocation: string | null
  painSide: string | null
  painIntensity: number | null
  stiffness: string | null
  swelling: string | null
  instability: boolean
  locking: boolean
  expectedWorkLoad: number
  plannedCyclingKm: number | null
  plannedActivity: string | null
  notes: string | null
  needsAttention: boolean
}

export interface EvidenceSummary {
  draft: number
  inReview: number
  informative: number
  operational: number
}

export interface DailyAction { kind: string; title: string; detail: string; route: string; state: 'pending' | 'done' | 'active' | 'attention' | 'optional' }
export interface TodayTraining { id: string; name: string; activityType: string; status: string; plannedStartTime:string|null; plannedDurationMinutes: number | null; targetRpe: number | null }
export interface TodaySchedule{id:string;name:string;category:string;timeWindow:'exact'|'morning'|'afternoon'|'evening'|'flexible';startTime:string|null;endTime:string|null;notes:string|null}
export interface DailyStateFactor{key:string;label:string;current:number;baseline:number;delta:number;visualThreshold:number;unit:string;trend:'better'|'worse'|'similar';basis:string}
export interface DailyState{status:'incomplete'|'recorded'|'stable'|'observe'|'attention';label:string;summary:string;baselineDays:number;factors:DailyStateFactor[];disclaimer:string}
export interface DailyPlanContext{status:'none'|'active'|'attention'|'incomplete'|'observe'|'planned';label:string;summary:string;sessionCount:number;plannedMinutes:number;plannedLoad:number;incompleteSessions:number;hasInProgress:boolean;disclaimer:string}
export interface DailyPlanDecision{id:string;date:string;decision:'as-planned'|'adjusted'|'recovery'|'professional-review';reason:string;contextStatus:string;plannedLoadSnapshot:number;version:number;updatedAt:string}
export interface SaveDailyPlanDecisionRequest{decision:DailyPlanDecision['decision'];reason:string}
export interface DailyActivity { id: string; date: string; activityType: string; durationMinutes: number; rpe: number; distanceKm: number | null; notes: string | null; internalLoad: number; plannedDurationMinutes:number|null; plannedSource:string|null; durationVarianceMinutes:number|null; workDemands:string|null; breakMinutes:number|null; unusualConditions:string|null }
export interface SaveDailyActivityRequest { date: string; activityType: string; durationMinutes: number; rpe: number; distanceKm: number | null; notes: string; plannedDurationMinutes:number|null; plannedSource:string|null; workDemands:string|null; breakMinutes:number|null; unusualConditions:string|null }
export interface DailyHub { actions: DailyAction[]; state:DailyState; planContext:DailyPlanContext; decision:DailyPlanDecision|null; todaySessions: TodayTraining[]; todaySchedule:TodaySchedule[]; scheduledMinutes:number; hasScheduleConflict:boolean; pendingFollowUps: number; openLearningActions:number; dueLearningActions:number; oldestOpenLearningDate:string|null; activeMetrics: number; metricsWithoutEntries: number; daysToPrimaryTarget: number | null; todayActivities: DailyActivity[]; trainingLoadToday: number; externalLoadToday: number; totalLoadToday: number }

export interface AtlasOverview {
  profile: AtlasProfile
  facts: ProfileFact[]
  today: DailyCheckIn | null
  recentCheckIns: DailyCheckIn[]
  evidence: EvidenceSummary
  hub: DailyHub
}

export type UpdateAtlasProfileRequest = Omit<AtlasProfile, 'id'>
export type SaveDailyCheckInRequest = Omit<DailyCheckIn, 'id' | 'needsAttention'>
