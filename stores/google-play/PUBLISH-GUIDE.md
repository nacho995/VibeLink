# Guía de Publicación en Google Play Store

## Requisitos Previos

1. **Cuenta de Google Play Developer** ($25 único)
   - https://play.google.com/console/signup

2. **Keystore para firmar la app** (lo creamos abajo)

3. **Assets gráficos**:
   - Icono 512x512 PNG (ya tienes el SVG)
   - Feature Graphic 1024x500
   - Screenshots (mínimo 2, recomendado 6-8)
   - Video promocional (opcional, YouTube)

---

## Paso 1: Crear Keystore de Producción

```bash
# Ejecutar en macOS (donde puedes compilar)
cd ~/Documentos/Programacion/VibeLink

# Crear keystore (GUARDA LA CONTRASEÑA EN LUGAR SEGURO)
keytool -genkey -v \
  -keystore stores/google-play/vibelink-release.keystore \
  -alias vibelink \
  -keyalg RSA \
  -keysize 2048 \
  -validity 10000

# Te pedirá:
# - Contraseña del keystore
# - Nombre y apellidos
# - Unidad organizativa
# - Organización
# - Ciudad
# - Provincia
# - Código de país (ES)
```

**IMPORTANTE**: 
- Guarda el keystore y la contraseña en lugar seguro
- Si los pierdes, NO podrás actualizar la app nunca más
- Haz backup en un lugar seguro (no en git)

---

## Paso 2: Configurar Firma en el Proyecto

Editar `frontend/VibeLink.App/VibeLink.App.csproj`, añadir dentro de `<PropertyGroup>`:

```xml
<!-- Android Signing -->
<AndroidKeyStore>true</AndroidKeyStore>
<AndroidSigningKeyStore>../../stores/google-play/vibelink-release.keystore</AndroidSigningKeyStore>
<AndroidSigningKeyAlias>vibelink</AndroidSigningKeyAlias>
<AndroidSigningKeyPass>TU_CONTRASEÑA</AndroidSigningKeyPass>
<AndroidSigningStorePass>TU_CONTRASEÑA</AndroidSigningStorePass>
```

**Nota**: Para CI/CD, usa variables de entorno en lugar de hardcodear contraseñas.

---

## Paso 3: Compilar AAB para Play Store

```bash
cd frontend/VibeLink.App

# Compilar Android App Bundle (requerido por Play Store)
dotnet publish -f net10.0-android \
  -c Release \
  -p:AndroidPackageFormat=aab

# El archivo estará en:
# bin/Release/net10.0-android/publish/com.companyname.vibelink.app-Signed.aab
```

---

## Paso 4: Crear App en Play Console

1. Ve a https://play.google.com/console
2. "Crear aplicación"
3. Rellenar:
   - Nombre: VibeLink - Match por Gustos
   - Idioma: Español (España)
   - Tipo: Aplicación
   - Gratuita/De pago: Gratuita (con compras in-app)

---

## Paso 5: Configuración de la Ficha

### Ficha de Play Store Principal
- Copiar contenido de `store-listing.md`
- Subir screenshots
- Subir feature graphic

### Categorización
- Categoría: Social
- Etiquetas: Dating, Entertainment, Social

### Datos de contacto
- Email: soporte@vibelink.app
- Sitio web: https://vibelink.app (opcional)

---

## Paso 6: Política de Privacidad

Crear página en: https://vibelink-api.fly.dev/privacy

Contenido mínimo requerido:
- Qué datos recopilas
- Cómo los usas
- Con quién los compartes
- Cómo pueden eliminar su cuenta

---

## Paso 7: Clasificación de Contenido

Completar cuestionario de clasificación:
- Violencia: No
- Contenido sexual: No
- Lenguaje soez: No
- Sustancias controladas: No

Resultado esperado: PEGI 12 / Teen

---

## Paso 8: Configurar Pagos (Stripe)

Como usas Stripe (no Google Play Billing):
- En "Monetización" > "Productos" indicar que usas facturación externa
- Google permite esto para apps que existían antes de 2022 o en ciertos casos
- Alternativa: Implementar Google Play Billing para suscripciones

---

## Paso 9: Pruebas Internas

1. Ir a "Pruebas" > "Pruebas internas"
2. Crear nueva versión
3. Subir AAB
4. Añadir testers (emails)
5. Publicar en pruebas internas

---

## Paso 10: Producción

1. Asegurarte de que todo está completo:
   - [ ] Ficha de Play Store
   - [ ] Clasificación de contenido
   - [ ] Política de privacidad
   - [ ] Precios y distribución
   - [ ] App firmada y subida

2. Ir a "Producción" > "Crear nueva versión"
3. Subir AAB
4. Escribir notas de la versión
5. "Iniciar lanzamiento en producción"

**Revisión**: Google tarda 1-7 días en revisar apps nuevas.

---

## Checklist Final

- [ ] Cuenta de desarrollador creada ($25)
- [ ] Keystore creado y respaldado
- [ ] AAB compilado y firmado
- [ ] Icono 512x512
- [ ] Feature graphic 1024x500
- [ ] Mínimo 2 screenshots
- [ ] Descripción corta y larga
- [ ] Política de privacidad online
- [ ] Clasificación de contenido completada
- [ ] Pruebas internas realizadas
- [ ] Enviado a producción
