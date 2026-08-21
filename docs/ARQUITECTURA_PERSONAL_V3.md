# Arquitectura funcional — Sistema Personal de Rendimiento V3

## 1. Nueva definición del producto

La aplicación deja de estar centrada en la administración de un gimnasio y pasa a ser el sistema personal de rendimiento de Jonathan.

Su función principal es conectar:

**estímulo → respuesta → recuperación → adaptación**

El sistema debe ayudar a responder cuatro preguntas:

1. ¿Cómo estoy hoy?
2. ¿Qué carga recibió mi cuerpo?
3. ¿Cómo respondió durante las 48 horas posteriores?
4. ¿Estoy desarrollando capacidad sin pagar un costo físico desproporcionado?

La aplicación registra hechos, calcula tendencias y presenta hipótesis. No diagnostica lesiones ni prescribe tratamientos.

## 2. Principios obligatorios

- Jonathan es el propietario de sus datos y decisiones.
- Registrar poco y de forma constante vale más que completar formularios extensos de manera irregular.
- Hecho observado, cálculo, hipótesis y recomendación deben ser capas separadas.
- Un dato aislado no decide; se observan tendencias, contexto y calidad de la información.
- La ausencia de un dato nunca se interpreta como cero.
- Toda modificación relevante conserva fecha, autor, valor anterior y motivo.
- La salud articular y la capacidad funcional tienen prioridad sobre récords, estética o gamificación.
- Las alertas deben explicar qué observaron, con qué datos y cuál es el siguiente paso prudente.
- Toda regla relacionada con salud, síntomas, recuperación o progresión debe tener una ficha de evidencia vigente.
- Una API, un estudio aislado o una correlación personal nunca se convierten automáticamente en una recomendación.

## 3. Perfil inicial incorporado

### Datos disponibles

- Titular: Jonathan.
- Altura aproximada autorreportada: 1,80 m.
- Peso de referencia aproximado: 65 kg.
- Peso histórico en una etapa entrenada: aproximadamente 70 kg.
- Actividades actuales: taekwondo, bicicleta como transporte y trabajo físico activo.
- Transporte habitual informado: aproximadamente 5 km diarios en bicicleta.
- Trabajo: limpieza, caminata, tiempo de pie, escaleras y traslado de objetos o baldes.
- Historia deportiva: fútbol como arquero y jugador de campo; capacidades históricas de velocidad, reacción y resistencia.
- Objetivo temporal declarado: prueba física policial prevista para mayo de 2027.
- Antecedentes autorreportados: lesión de LCA, lesión meniscal, hiperlaxitud generalizada y fractura de mano.

### Datos que deben permanecer pendientes

- Edad o fecha de nacimiento.
- Rodilla afectada y lateralidad completa.
- Fecha, mecanismo, diagnóstico documentado y estado clínico actual de LCA y menisco.
- Evaluación profesional de hiperlaxitud y articulaciones sintomáticas.
- Detalles y posibles secuelas de la fractura de mano.
- Reglamentación oficial, pruebas, mínimos y sistema de puntuación policial de 2027.
- Línea de base física estandarizada y síntomas actuales registrados de forma longitudinal.

Cada dato del perfil tendrá un estado: `confirmado`, `autorreportado`, `hipótesis`, `pendiente` o `validado por profesional`.

## 4. Usuarios y permisos

### Propietario / atleta

Puede ver, crear, editar, exportar y eliminar sus datos; configurar alertas; invitar o revocar profesionales; y decidir qué módulos comparte.

### Preparador físico o profesor

Puede crear y editar planes, rutinas y ejercicios; revisar adherencia, carga y respuesta funcional; comentar; y proponer cambios. No accede a documentos clínicos, fotografías ni antecedentes sensibles sin permiso explícito.

### Kinesiólogo, fisioterapeuta o médico

Puede acceder temporalmente a los módulos autorizados, añadir evaluaciones profesionales y revisar cronologías. Sus aportes quedan identificados como profesionales y no reemplazan el dato original del atleta.

### Acceso de solo lectura

Permite compartir un informe o período concreto con fecha de vencimiento, revocación inmediata y registro de accesos.

## 5. Módulos definitivos

### 5.1 Hoy

- Check-in adaptativo de 60 a 90 segundos.
- Sueño, energía, fatiga, estrés y molestias.
- Plan del día y carga laboral prevista.
- Seguimientos pendientes de 24 o 48 horas.
- Alertas explicables y una acción principal.

### 5.2 Plan y entrenamientos

- Objetivos, ciclos, semanas y sesiones planificadas.
- Rutinas editables por Jonathan o un profesional autorizado.
- Registro específico para gimnasio, taekwondo, carrera, bicicleta, fútbol y movilidad/rehabilitación.
- Comparación entre planificado, completado, modificado, reprogramado u omitido.
- Historial de versiones de rutina y motivo de cada cambio.

### 5.3 Cuerpo, síntomas y función

- Mapa corporal por zona, articulación y lado.
- Dolor, rigidez, inflamación, calor, bloqueo, chasquido doloroso, falseo y confianza.
- Función en caminar, escaleras, apoyo unilateral, giro, salto, patada y tareas personalizadas.
- Seguimiento durante, después, a las 24 horas y a las 48 horas.
- Adjuntos opcionales: informes, fotografías y videos, con privacidad reforzada.

### 5.4 Rodilla e hiperlaxitud

- Cronología específica de rodilla.
- Respuesta por exposición y retorno a la línea de base.
- Registro articular bilateral para hiperlaxitud: rango, control activo, síntomas y estrategia utilizada.
- Diferenciación entre movilidad activa, movilidad pasiva y estabilidad.
- Semáforo descriptivo con texto y evidencia; nunca color aislado ni diagnóstico automático.

### 5.5 Trabajo, transporte y vida diaria

- Plantillas de jornada liviana, habitual o pesada.
- Horas, escaleras, tiempo de pie, cargas, asimetrías, pausas, calzado y superficie.
- Bicicleta de traslado separada de bicicleta como entrenamiento.
- Registro rápido de estrés, enfermedad, viajes o cambios extraordinarios.

### 5.6 Recuperación, hábitos y composición corporal

- Sueño, fatiga, dolor muscular, energía, estrés, apetito, hidratación y tolerancia digestiva.
- Peso por promedio móvil, perímetros con protocolo y fotos estandarizadas opcionales.
- Método y margen de error de cualquier estimación corporal.
- Posibilidad de ocultar el módulo de composición corporal.

### 5.7 Pruebas y capacidades

- Protocolos versionados para fuerza, resistencia, velocidad, agilidad, equilibrio, control, potencia y movilidad.
- Resultado, unidad, lado, calentamiento, superficie, calzado, hora, intentos y motivo de interrupción.
- Estado del resultado: válido, dudoso o inválido.
- Comparación con la línea de base personal; nunca mezclar protocolos incompatibles.

### 5.8 Objetivos y hoja de ruta

- Jerarquía: visión, resultado, capacidad, proceso y guardarraíl de seguridad.
- Hitos y criterios de avance por fase.
- Tres ejes de progreso: rendimiento, consistencia y tolerancia.
- La preparación policial específica queda bloqueada como pendiente hasta cargar el reglamento oficial vigente.

### 5.9 Línea de tiempo e historial

- Vista diaria, semanal, mensual y por episodio.
- Superposición de entrenamiento, trabajo, transporte, síntomas, recuperación, pruebas e intervenciones.
- Filtros por modalidad, lado, ejercicio, superficie, calzado, objetivo y profesional.
- Comparación de períodos conservando el contexto y la calidad de los datos.

### 5.10 Bitácora de aprendizaje

Cada entrada separa:

1. Qué observé.
2. Qué creo que significa.
3. Qué voy a cambiar.
4. Cómo sabré si funcionó.

Esto permite crear experimentos personales controlados sin confundir correlación con causalidad.

### 5.11 Profesionales e informes

- Invitaciones granulares y temporales.
- Comentarios y propuestas de cambio.
- Informe semanal personal.
- Informe de rodilla o de un episodio.
- Informe para profesional con período y módulos elegidos.
- Exportación completa en formatos legibles y estructurados.

### 5.12 Privacidad y auditoría

- Consentimiento por módulo, adjunto, sensor y profesional.
- Historial de accesos y modificaciones.
- Sesiones revocables.
- Corrección, exportación y eliminación accesibles.
- Separación de datos generales, salud sensible y foto/video.

## 6. Flujo diario principal

### Mañana

1. Check-in breve.
2. Si no hay síntomas, el flujo termina rápidamente.
3. Si existe una molestia, aparecen preguntas por zona, lado, intensidad y función.
4. Se muestra el plan del día junto con el contexto, sin una puntuación opaca de “apto/no apto”.

### Durante el día

1. Registrar jornada laboral mediante una plantilla.
2. Registrar bicicleta como transporte o entrenamiento.
3. Iniciar o completar una sesión planificada.

### Cierre de sesión

1. Confirmar contenido y duración real.
2. Registrar RPE, técnica, fatiga local y dolor durante/después.
3. Marcar el objetivo como cumplido, parcial, modificado o no realizado.
4. Programar automáticamente el seguimiento de 24 horas cuando corresponda.

### Día siguiente

1. Pregunta adaptativa sobre síntoma, inflamación, función y confianza.
2. Comparación con la línea de base.
3. Cierre del episodio o seguimiento hasta 48 horas.

### Fin de semana

1. Resumen de carga por modalidad y demanda mecánica.
2. Mejor respuesta y señal a vigilar.
3. Adherencia incluyendo modificaciones inteligentes.
4. Aprendizaje de la semana.
5. Decisión documentada para la semana siguiente.

## 7. Motor de análisis

### Métricas iniciales

- Carga de sesión: minutos × RPE, conservando la modalidad.
- Carga semanal por entrenamiento, trabajo y transporte.
- Tendencia de sueño, energía, fatiga y síntomas.
- Tiempo de retorno del síntoma a la línea de base.
- Adherencia flexible al plan.
- Tolerancia a una exposición definida.
- Asimetría bajo el mismo protocolo.
- Cobertura y calidad de los datos.

### Reglas

- No sumar como equivalentes impactos, fuerza, cambios de dirección y bicicleta.
- No concluir causalidad a partir de una coincidencia temporal.
- Mostrar tamaño de muestra, datos faltantes y nivel de confianza.
- Permitir confirmar, descartar o comentar cada hipótesis.
- No crear una puntuación única que oculte los componentes.

## 8. Alertas y seguridad

Cada alerta contiene: observación, ventana temporal, evidencia usada, datos faltantes, confianza, carácter no diagnóstico y siguiente acción prudente.

Categorías iniciales:

- Calidad o incoherencia del dato.
- Cambio marcado de carga respecto del rango personal.
- Síntoma que no vuelve a la línea de base.
- Episodios de inestabilidad o bloqueo.
- Recuperación baja junto con demanda elevada.
- Objetivo en riesgo.
- Logro de consistencia, capacidad o tolerancia.

Ante trauma importante, incapacidad para apoyar, deformidad, bloqueo súbito, derrame marcado o persistente, dolor intenso o pérdida notable de movilidad, la aplicación debe priorizar un mensaje de atención profesional y detener cualquier automatización de progresión.

## 9. Modelo de datos conceptual

### Identidad y evidencia

- `AthleteProfile`
- `ProfileFact`
- `EvidenceSource`
- `MedicalHistoryItem`
- `ProfessionalDocument`
- `Consent`
- `AccessGrant`
- `AuditEvent`

### Línea temporal

- `TimelineEvent`
- `DailyCheckIn`
- `TrainingSession`
- `SessionBlock`
- `ExerciseEntry`
- `WorkExposure`
- `TransportExposure`
- `LifeContextEvent`
- `SymptomObservation`
- `FollowUpResponse`

### Planificación y desarrollo

- `Goal`
- `Milestone`
- `TrainingPlan`
- `PlanCycle`
- `PlanWeek`
- `PlannedSession`
- `PlanRevision`
- `Intervention`

### Evaluaciones

- `BodyMeasurement`
- `PhotoSet`
- `JointAssessment`
- `MovementAssessment`
- `TestProtocol`
- `TestResult`

### Aprendizaje y analítica

- `MetricDefinition`
- `MetricValue`
- `Hypothesis`
- `Alert`
- `WeeklyReview`
- `MonthlyReview`
- `LearningEntry`
- `ReportSnapshot`

### Evidencia científica y clínica

- `EvidenceSource`
- `EvidenceDocument`
- `EvidenceAssessment`
- `EvidenceStatement`
- `ClinicalRule`
- `RuleEvidenceLink`
- `EvidenceReview`
- `TerminologyConcept`
- `ExternalImportLog`

Los registros temporales comparten fecha, duración, origen, calidad, privacidad, autor y versión. Las entidades específicas agregan sus propios campos sin convertir todo en una tabla gigante.

## 10. Reutilización de la base actual

| Base V2 | Destino V3 |
|---|---|
| Autenticación y usuarios | Se conserva y refuerza |
| `Member` | `AthleteProfile` |
| `TrainerAssignment` | `AccessGrant` granular |
| `WorkoutRoutine` | `TrainingPlan` versionado |
| `WorkoutDay` | `PlanWeek` + `PlannedSession` |
| `WorkoutExercise` | `ExerciseEntry` y prescripción planificada |
| `WorkoutProgress` | Ejecución real, respuesta y adherencia |
| Reportes | `ReportSnapshot` con selección y trazabilidad |

Membresías, pagos y clases grupales no forman parte del núcleo personal inicial. Se conservarán aislados para no perder el trabajo existente, pero no condicionarán el nuevo modelo.

## 11. Arquitectura técnica propuesta

- Frontend: Vue 3, TypeScript y diseño mobile-first.
- Primera experiencia móvil: PWA instalable, rápida y usable con una mano.
- Evolución posterior: empaquetado móvil con Capacitor si se necesitan sensores, cámara o notificaciones nativas más profundas.
- Backend: API .NET existente, reorganizada por módulos de dominio.
- Persistencia: conservar la base actual durante la transición y crear migraciones nuevas sin borrar el historial.
- Adjuntos sensibles: almacenamiento separado, acceso firmado y cifrado.
- Analítica: cálculos versionados y reproducibles en el servidor.
- Sin conexión: check-in y cierre de sesión con cola local y sincronización segura en una etapa posterior.

## 12. Ruta de implementación

### Etapa 0 — Confirmar la línea de base

- Cargar el perfil inicial con estados de evidencia.
- Completar los datos personales prioritarios.
- Adjuntar documentación clínica solo si Jonathan decide incorporarla.
- Confirmar el reglamento policial antes de definir pruebas específicas.

### Etapa 1 — Núcleo personal

- Nuevo inicio “Hoy”.
- Check-in adaptativo.
- Línea de tiempo.
- Registro laboral y bicicleta.
- Cierre de sesión y seguimiento a 24 horas.

### Etapa 2 — Rodilla y respuesta

- Mapa corporal y función.
- Seguimiento 0-48 horas.
- Alertas de seguridad explicables.
- Resumen semanal.

### Etapa 3 — Planes editables

- Objetivos, ciclos y rutinas versionadas.
- Edición por atleta, administrador personal o profesional invitado.
- Planificado frente a ejecutado.
- Registro del motivo de cada cambio.

### Etapa 4 — Evaluación y aprendizaje

- Pruebas protocolizadas.
- Composición corporal opcional.
- Bitácora de aprendizaje.
- Tendencias de 4, 8 y 12 semanas.

### Etapa 5 — Colaboración e informes

- Permisos granulares.
- Informes temporales para profesionales.
- Comentarios, propuestas y auditoría.

### Etapa 6 — Aplicación móvil e integraciones

- PWA instalable y notificaciones.
- Cámara y adjuntos.
- Salud del teléfono y wearables mediante adaptadores.
- Importaciones trazables y reversibles.

### Etapa 7 — Inteligencia personal

- Hipótesis basadas en historial suficiente.
- Comparaciones N-of-1.
- Recomendaciones explicables con incertidumbre visible.
- Nunca diagnóstico automático ni predicción de lesión presentada como certeza.

## 13. Primer incremento a construir

El primer incremento funcional debe incluir solo seis piezas conectadas de extremo a extremo:

1. Perfil con calidad y fuente de cada dato.
2. Check-in diario adaptativo.
3. Registro de trabajo, bicicleta y entrenamiento.
4. Cierre de sesión con respuesta inmediata.
5. Seguimiento de rodilla a las 24 horas.
6. Resumen semanal con una decisión documentada.

Si este ciclo resulta rápido, comprensible y sostenible durante varias semanas, la base estará lista para añadir pruebas, profesionales, integraciones y analítica avanzada.

## 14. Gobernanza científica y médica

La evidencia no será una sección decorativa. Formará parte del modelo de datos, del proceso de desarrollo y de cada función que pueda influir sobre entrenamiento, recuperación o consulta profesional.

### 14.1 Capas que nunca deben mezclarse

1. **Dato personal observado:** lo que Jonathan registró o lo que ingresó un dispositivo.
2. **Métrica calculada:** fórmula reproducible aplicada a datos identificados.
3. **Asociación personal:** patrón temporal detectado en el historial.
4. **Evidencia externa:** estudio, revisión sistemática, guía o consenso.
5. **Regla aprobada:** comportamiento de la aplicación revisado y versionado.
6. **Decisión humana:** elección de Jonathan o de un profesional autorizado.

La interfaz debe indicar siempre en qué capa se encuentra cada afirmación.

### 14.2 Jerarquía práctica de fuentes

La prioridad general será:

1. Guías clínicas o de salud pública vigentes, desarrolladas con metodología explícita.
2. Revisiones sistemáticas y metaanálisis de calidad y aplicabilidad suficientes.
3. Consensos o posicionamientos de organizaciones científicas reconocidas.
4. Ensayos controlados y estudios prospectivos relevantes.
5. Estudios observacionales.
6. Opinión experta, hipótesis biomecánica o razonamiento indirecto claramente identificado.

Esta jerarquía no se aplicará de forma mecánica. El diseño adecuado depende de la pregunta; además se evaluarán sesgo, precisión, consistencia, población, intervención, comparador, resultados, daños y aplicabilidad al caso personal.

### 14.3 Ficha obligatoria de evidencia

Toda afirmación o regla importante deberá conservar:

- Pregunta clínica o deportiva concreta.
- Módulo y función afectada.
- Fuente primaria y enlace permanente, DOI, PMID o identificador oficial.
- Tipo de documento y diseño de estudio.
- Fecha de publicación, versión y fecha de consulta.
- Población estudiada y diferencias respecto de Jonathan.
- Resultado, magnitud del efecto y daños considerados.
- Riesgo de sesgo y certeza de la evidencia.
- Aplicabilidad: directa, indirecta o no determinada.
- Conflictos de interés y financiación declarados cuando estén disponibles.
- Redacción exacta que puede mostrar la aplicación.
- Límites y situaciones en las que no debe aplicarse.
- Profesional o revisor responsable.
- Próxima fecha de revisión y motivo de retiro si queda obsoleta.

### 14.4 Estados de una regla

- `borrador`: en investigación; no aparece al usuario.
- `en revisión`: evaluada técnicamente, pendiente de validación.
- `aprobada informativa`: puede explicar o educar, pero no modificar un plan.
- `aprobada operativa`: puede generar una alerta o propuesta dentro de límites definidos.
- `suspendida`: existe nueva evidencia, conflicto o problema de seguridad.
- `retirada`: ya no debe ejecutarse; se conserva por auditoría.

Una actualización de evidencia nunca cambia retrospectivamente qué regla se utilizó en una decisión pasada.

### 14.5 Tipos de salida permitidos

La evidencia puede alimentar:

- Explicaciones educativas.
- Preguntas de seguimiento.
- Validaciones de unidades o protocolos.
- Recordatorios de datos faltantes.
- Alertas prudentes para observar, ajustar con un profesional o consultar.
- Informes con citas y limitaciones.

No puede alimentar automáticamente:

- Diagnósticos.
- Prescripción de rehabilitación o tratamiento.
- Predicción individual de lesión presentada como certeza.
- Modificación silenciosa de una rutina.
- Recomendaciones basadas únicamente en un artículo, ensayo en curso o correlación personal.

## 15. Dossiers científicos por módulo

Antes de implementar reglas avanzadas, cada módulo tendrá un dossier propio.

### Perfil, antecedentes y seguridad

- Definiciones normalizadas.
- Banderas de seguridad y límites del automanejo.
- Criterios de derivación redactados y revisados profesionalmente.

### Entrenamiento y carga

- Validez y límites de RPE, RIR y carga minutos × RPE.
- Especificidad por modalidad y demanda mecánica.
- Progresión, fatiga y adherencia sin utilizar umbrales universales no validados.

### Rodilla, LCA y menisco

- Resultados funcionales, síntomas e inestabilidad.
- Respuesta inmediata y tardía a la exposición.
- Criterios de evaluación profesional y retorno a actividades, sin convertirlos en un alta automática.

### Hiperlaxitud

- Diferenciación entre rango, síntomas, control y función.
- Evaluaciones válidas y límites del autorreporte.
- Evitar la suposición de que más movilidad es necesariamente mejor o peor.

### Sueño y recuperación

- Instrumentos de autorreporte y métricas interpretables.
- Relación con rendimiento y bienestar sin diagnosticar trastornos del sueño.

### Composición corporal y nutrición

- Protocolos de medición, error y tendencia.
- Lenguaje no moralizante y protección frente a conclusiones basadas en cambios diarios.
- Cualquier objetivo nutricional clínico queda bajo un profesional habilitado.

### Pruebas de rendimiento

- Fiabilidad, error de medición, familiarización y comparabilidad.
- Protocolos específicos y criterios de interrupción.
- Separación entre mejora real y variación normal.

## 16. Fuentes e integraciones externas propuestas

### Primera prioridad: consulta y curación

- **PubMed / NCBI E-utilities:** búsqueda y recuperación de referencias biomédicas. Se usará para descubrir literatura y mantener bibliografía, no para generar recomendaciones automáticas.
- **Guías de OMS/OPS y organismos profesionales:** fuente de recomendaciones ya desarrolladas mediante procesos explícitos. Su incorporación será manual, contextualizada y versionada.
- **ClinicalTrials.gov API:** seguimiento de estudios y resultados registrados. Un ensayo en curso o un registro sin resultados no se considera evidencia suficiente para una regla.

### Segunda prioridad: interoperabilidad y terminología

- **HL7 FHIR:** formato de intercambio para observaciones, informes, profesionales, consentimientos, procedencia y auditoría.
- **WHO ICD-11 API:** clasificación interoperable cuando corresponda. Un código nunca será usado por la aplicación para diagnosticar.
- **LOINC:** codificación de mediciones, cuestionarios u observaciones estandarizadas. Su servicio FHIR público se evaluará como fuente de desarrollo, no como dependencia clínica crítica mientras mantenga estado beta.
- **SNOMED CT:** terminología clínica detallada, sujeta a revisión de licencias, edición nacional y disponibilidad para el país de uso.

### Tercera prioridad: dispositivos y salud móvil

- Integraciones de actividad, sueño o frecuencia cardíaca mediante adaptadores.
- Cada dato importado conserva dispositivo, algoritmo si se conoce, unidad, zona horaria, fecha y calidad.
- Los datos de distintos dispositivos no se consideran intercambiables sin validación.

## 17. Canal de actualización científica

1. Definir una pregunta concreta del producto.
2. Buscar guías y revisiones; documentar estrategia y fecha.
3. Evaluar calidad, aplicabilidad y posibles daños.
4. Redactar una ficha de evidencia en lenguaje técnico y otra versión para el usuario.
5. Vincularla a una regla en estado borrador.
6. Revisarla con el profesional correspondiente cuando pueda afectar conducta o seguridad.
7. Probar la regla con casos simulados, incluidos datos faltantes y escenarios adversos.
8. Publicarla con versión, explicación y fecha de revisión.
9. Monitorizar resultados inesperados y permitir reportar una recomendación poco útil.
10. Suspenderla automáticamente si vence la revisión o cambia una fuente crítica, hasta una nueva evaluación.

## 18. Primer backlog de investigación

El desarrollo científico comenzará en este orden:

1. Señales de seguridad y seguimiento de rodilla.
2. Uso y límites de RPE, RIR y carga de sesión.
3. Respuesta de síntomas a 24 y 48 horas.
4. Carga laboral y recuperación.
5. Sueño, energía y fatiga autorreportada.
6. Pruebas iniciales de control, fuerza y capacidad aeróbica.
7. Hiperlaxitud, control activo y función.
8. Requisitos oficiales de la prueba policial cuando se publique la convocatoria aplicable.

Cada punto producirá un dossier revisable antes de incorporarse como lógica activa de la aplicación.
