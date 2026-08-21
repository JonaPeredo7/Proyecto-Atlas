import type{WeeklyReport}from'../types/weekly'
export async function getWeeklyReport(){const r=await fetch('/api/insights/weekly',{credentials:'include'});if(!r.ok){const b=await r.json().catch(()=>null)as{message?:string}|null;throw new Error(b?.message??'No se pudo generar el resumen semanal.')}return r.json()as Promise<WeeklyReport>}
