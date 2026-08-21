# Dossier científico: sueño, energía y fatiga autorreportada

Estado: **informativo**  
Revisión documental: 15 de agosto de 2026  
Próxima revisión: 15 de agosto de 2027

## Pregunta del producto

¿Cómo debe presentar Proyecto Atlas el sueño, la energía, la fatiga, el estrés y el dolor diarios para aportar contexto útil sin diagnosticar ni decidir automáticamente si Jonathan debe entrenar?

## Decisión vigente

- Atlas compara el registro del día con la referencia personal reciente formada por al menos tres días anteriores comparables.
- La duración del sueño y la calidad percibida se conservan como datos distintos.
- Energía, fatiga, estrés y dolor son autorreportes de contexto; no son pruebas diagnósticas ni mediciones objetivas de recuperación.
- Los cortes que cambian el color de cada tarjeta son reglas visuales explícitas de Atlas: 1 punto para calidad del sueño, energía y estrés; 2 puntos para fatiga y dolor; 1 hora para duración del sueño.
- Dos factores menos favorables activan el estado “Observar”. Esta combinación es una heurística interna y no un umbral clínico validado.
- Una señal o una asociación temporal nunca modifica el entrenamiento por sí sola. La decisión queda registrada por la persona y puede incluir consulta profesional.

## Evidencia curada

### Consenso de sueño para adultos sanos

- Fuente: [PubMed PMID 25979105](https://pubmed.ncbi.nlm.nih.gov/25979105/), DOI 10.5664/jcsm.4758.
- Diseño: declaración de consenso AASM/SRS mediante un proceso RAND modificado.
- Hallazgo aplicable: para adultos de 18 a 60 años, dormir regularmente siete horas o más se asocia con salud óptima a nivel general.
- Límite: es orientación poblacional de salud, no una frontera diaria que determine recuperación deportiva o aptitud para entrenar.

### Consenso experto sobre sueño del deportista

- Fuente: [PubMed PMID 33144349](https://pubmed.ncbi.nlm.nih.gov/33144349/), DOI 10.1136/bjsports-2020-102025.
- Diseño: revisión narrativa y consenso experto.
- Hallazgo aplicable: recomienda evaluar el sueño del deportista de forma individual y considerar la necesidad percibida, el contexto y los obstáculos específicos.
- Límite: el propio consenso reconoce limitaciones metodológicas en la literatura y no aporta una regla universal capaz de decidir una sesión individual.

### Medidas breves de bienestar en deportistas

- Fuente: [PubMed PMID 32991706](https://pubmed.ncbi.nlm.nih.gov/32991706/), DOI 10.4085/1062-6050-0528.19.
- Diseño: revisión sistemática de medidas autorreportadas de un solo ítem.
- Hallazgo aplicable: fatiga, calidad del sueño, estrés y estado de ánimo son variables habituales y factibles para seguimiento frecuente.
- Límite: las asociaciones con la carga de entrenamiento fueron heterogéneas y, en los conjuntos de datos mayores, principalmente triviales a moderadas. No justifica predicción causal ni diagnóstico.

### Seguimiento subjetivo repetido de la respuesta al entrenamiento

- Fuente: [PubMed PMID 26423706](https://pubmed.ncbi.nlm.nih.gov/26423706/), DOI 10.1136/bjsports-2015-094758.
- Diseño: revisión sistemática de medidas subjetivas y objetivas de bienestar en deportistas.
- Hallazgo aplicable: respalda el valor práctico del autorreporte repetido y la necesidad de formar una referencia individual antes de interpretar cambios.
- Límite: las medidas subjetivas y objetivas no son intercambiables. La revisión no valida los cortes visuales internos de Atlas ni una decisión clínica automática.

### Variabilidad del sueño dentro de una misma persona

- Fuente: [PubMed PMID 37485972](https://pubmed.ncbi.nlm.nih.gov/37485972/), DOI 10.1111/sms.14453.
- Diseño: revisión sistemática con síntesis narrativa.
- Hallazgo aplicable: la variación intraindividual del sueño es un objeto de seguimiento relevante y requiere registros repetidos.
- Límite: los estudios emplearon definiciones y formas de cálculo heterogéneas. Atlas documenta su propio método y no convierte el rango observado en normalidad clínica.

## Reglas de interfaz y cálculo

| Factor | Escala | Diferencia visual | Dirección favorable |
|---|---:|---:|---|
| Calidad del sueño | 1–5 | 1 punto | Mayor |
| Energía | 1–5 | 1 punto | Mayor |
| Fatiga | 1–10 | 2 puntos | Menor |
| Estrés | 1–5 | 1 punto | Menor |
| Dolor | 0–10 | 2 puntos | Menor |
| Duración del sueño | horas | 1 hora | Mayor |

Los cortes anteriores controlan solamente la clasificación visual “similar / más favorable / menos favorable”. Deben mostrarse junto al resultado y revisarse cuando haya suficientes datos personales o validación profesional. No representan diferencias mínimas clínicamente importantes.

## Perfil descriptivo de 28 días

- Se incluyen únicamente check-ins dentro de los últimos 28 días.
- Para cada variable se informa por separado el número de registros disponibles; los valores faltantes no se sustituyen por cero.
- La mediana se calcula como percentil 50.
- El rango central se calcula entre los percentiles 25 y 75 mediante interpolación lineal sobre los valores ordenados.
- También se muestran el mínimo y el máximo observados, además de la cobertura total de check-ins sobre 28 días.
- Este rango describe el 50% central de lo registrado. No es una “zona ideal”, un intervalo de referencia médica ni un criterio para autorizar entrenamiento.

## Comparación reciente contra referencia previa

- El período reciente comprende los últimos 7 días y la referencia los 21 días inmediatamente anteriores; no se superponen.
- Se compara la mediana reciente con los percentiles 25 y 75 del período previo.
- “Por encima” y “por debajo” describen posición numérica, no una valoración favorable o desfavorable.
- Para mostrar posición se exigen al menos 3 valores recientes y 7 previos por indicador. Este requisito es una regla de cobertura de Atlas y no validación estadística o clínica.
- Con menor cobertura se muestran las cantidades disponibles y el estado “Faltan datos”, sin clasificación.
- La comparación no demuestra que el entrenamiento, el trabajo, el sueño u otro factor haya causado el cambio.

## Salidas permitidas

- Describir cambios respecto de la referencia personal y mostrar el tamaño de la diferencia.
- Señalar datos faltantes y la cantidad de días que forma la referencia.
- Invitar a revisar el contexto, el plan y la evolución de varios días.
- Mantener visibles las señales de síntomas que requieren seguimiento.

## Salidas no permitidas

- Diagnosticar privación de sueño, sobreentrenamiento, lesión o enfermedad.
- Afirmar que un factor causó otro por coincidir en el tiempo.
- Cancelar, aumentar o reducir automáticamente una sesión.
- Usar siete horas como semáforo individual rígido o como autorización para entrenar.
- Presentar las reglas visuales internas como recomendaciones médicas.

## Próxima validación

Con una serie personal más extensa se podrá estudiar estabilidad, variabilidad habitual y datos faltantes. Antes de cambiar estas reglas por umbrales personalizados se deberán probar casos simulados, documentar el método y someter la propuesta a revisión profesional.
