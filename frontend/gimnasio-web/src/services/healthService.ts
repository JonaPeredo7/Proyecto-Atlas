import type{KneeCheck,KneeOverview,SaveKneeCheckRequest}from'../types/health'
async function request<T>(url:string,options?:RequestInit):Promise<T>{const r=await fetch(url,{...options,credentials:'include',headers:{'Content-Type':'application/json',...options?.headers}});if(!r.ok){const b=await r.json().catch(()=>null)as{message?:string}|null;throw new Error(b?.message??'No se pudo completar la operación.')}return r.json()as Promise<T>}
export const getKneeOverview=()=>request<KneeOverview>('/api/health/knee')
export const saveKneeCheck=(body:SaveKneeCheckRequest,id?:string)=>request<KneeCheck>(id?`/api/health/knee/checks/${id}`:'/api/health/knee/checks',{method:id?'PUT':'POST',body:JSON.stringify(body)})
export const deleteKneeCheck=async(id:string)=>{const r=await fetch(`/api/health/knee/checks/${id}`,{method:'DELETE',credentials:'include'});if(!r.ok)throw new Error('No se pudo eliminar el control.')}
