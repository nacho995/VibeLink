# Guía de Publicación en Apple App Store

## Requisitos Previos

1. **Apple Developer Program** ($99/año)
   - https://developer.apple.com/programs/enroll/

2. **Mac con Xcode** instalado (ya lo tienes)

3. **Certificados y perfiles**:
   - Distribution Certificate
   - App Store Provisioning Profile

---

## Paso 1: Configurar Apple Developer Account

1. Ir a https://developer.apple.com
2. Inscribirse en Apple Developer Program ($99/año)
3. Esperar aprobación (puede tardar 24-48h)

---

## Paso 2: Crear App ID y Certificados

### En Apple Developer Portal:

1. **Crear App ID**:
   - Ir a Certificates, Identifiers & Profiles
   - Identifiers > "+" 
   - App IDs > Continue
   - Bundle ID: `com.vibelink.app`
   - Description: VibeLink
   - Capabilities: Push Notifications (opcional)

2. **Crear Distribution Certificate**:
   - Certificates > "+"
   - Apple Distribution
   - Seguir instrucciones (crear CSR desde Keychain)

3. **Crear Provisioning Profile**:
   - Profiles > "+"
   - App Store
   - Seleccionar App ID: com.vibelink.app
   - Seleccionar Certificate
   - Nombre: VibeLink App Store
   - Descargar e instalar (doble click)

---

## Paso 3: Configurar el Proyecto

Editar `frontend/VibeLink.App/Platforms/iOS/Info.plist`:

```xml
<!-- Añadir estas claves -->
<key>CFBundleDisplayName</key>
<string>VibeLink</string>

<key>CFBundleIdentifier</key>
<string>com.vibelink.app</string>

<key>CFBundleShortVersionString</key>
<string>1.0.0</string>

<key>CFBundleVersion</key>
<string>1</string>

<key>ITSAppUsesNonExemptEncryption</key>
<false/>

<key>NSCameraUsageDescription</key>
<string>VibeLink necesita acceso a la cámara para tu foto de perfil</string>

<key>NSPhotoLibraryUsageDescription</key>
<string>VibeLink necesita acceso a tus fotos para tu foto de perfil</string>
```

---

## Paso 4: Compilar para App Store

```bash
cd frontend/VibeLink.App

# Compilar para iOS
dotnet publish -f net10.0-ios \
  -c Release \
  -p:ArchiveOnBuild=true \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="Apple Distribution: Tu Nombre (XXXXXXXXXX)" \
  -p:CodesignProvision="VibeLink App Store"
```

O mejor, usar Xcode:

```bash
# Generar proyecto para Xcode
dotnet build -f net10.0-ios -c Release

# Abrir en Xcode
open bin/Release/net10.0-ios/ios-arm64/VibeLink.App.app
```

---

## Paso 5: Archivar en Xcode

1. Abrir Xcode
2. Product > Archive
3. Esperar a que compile
4. En Organizer, seleccionar el archivo
5. "Distribute App" > "App Store Connect" > "Upload"

---

## Paso 6: Crear App en App Store Connect

1. Ir a https://appstoreconnect.apple.com
2. My Apps > "+"
3. Rellenar:
   - Platforms: iOS
   - Name: VibeLink - Match por Gustos
   - Primary Language: Spanish (Spain)
   - Bundle ID: com.vibelink.app
   - SKU: vibelink-ios-001

---

## Paso 7: Configurar la Ficha

### App Information
- Subtitle: Conecta por películas y juegos
- Category: Social Networking
- Secondary: Lifestyle
- Content Rights: No third-party content
- Age Rating: Complete questionnaire

### Pricing and Availability
- Price: Free
- Availability: All territories (o solo España)

### App Privacy
- Privacy Policy URL
- Data types collected
- Data linked to user

### In-App Purchases
1. Features > In-App Purchases > "+"
2. Type: Auto-Renewable Subscription
3. Reference Name: VibeLink Premium
4. Product ID: com.vibelink.app.premium.monthly
5. Subscription Group: VibeLink Premium
6. Price: €9.99

---

## Paso 8: Subir Screenshots

### Tamaños requeridos:
- 6.7" (iPhone 15 Pro Max): 1290 x 2796
- 6.5" (iPhone 14 Plus): 1284 x 2778
- 5.5" (iPhone 8 Plus): 1242 x 2208 (opcional)

### Tip: Usar Simulator
```bash
# Tomar screenshots desde simulator
xcrun simctl io booted screenshot screenshot.png
```

---

## Paso 9: Enviar para Revisión

1. Completar todos los campos requeridos
2. Subir build desde Xcode
3. Seleccionar build en App Store Connect
4. Añadir notas para revisión:
   ```
   Test account:
   Email: test@vibelink.app
   Password: Test123!
   
   This is a dating app where users connect based on 
   shared interests in movies, TV shows, and video games.
   ```
5. "Submit for Review"

---

## Paso 10: Revisión de Apple

- **Tiempo**: 24-48 horas (primera vez puede ser más)
- **Posibles rechazos comunes**:
  - Guideline 4.3: App similar a otras existentes
  - Guideline 5.1.1: Privacidad de datos
  - Guideline 3.1.1: Pagos fuera de App Store
  
### Sobre Pagos (IMPORTANTE):
Apple requiere usar In-App Purchase para suscripciones.
Stripe NO está permitido para compras digitales en iOS.

**Solución**: Implementar StoreKit para iOS.
```csharp
// En iOS, usar Plugin.InAppBilling
// https://github.com/jamesmontemagno/InAppBillingPlugin
```

---

## Checklist Final

- [ ] Apple Developer Program activo ($99/año)
- [ ] Certificado de distribución creado
- [ ] Provisioning Profile instalado
- [ ] Bundle ID configurado
- [ ] App compilada y archivada
- [ ] App Store Connect configurado
- [ ] Screenshots subidos (todas las resoluciones)
- [ ] Descripción y metadata
- [ ] Política de privacidad
- [ ] In-App Purchases configurados (StoreKit)
- [ ] Build subido
- [ ] Enviado para revisión

---

## Notas sobre Stripe vs StoreKit

Apple exige usar su sistema de pagos (30% comisión) para:
- Suscripciones digitales
- Contenido digital
- Funciones premium de la app

**Para VibeLink Premium**:
1. iOS: Usar StoreKit/In-App Purchase
2. Android: Puedes usar Stripe o Google Play Billing
3. Web: Stripe sin problemas

Esto significa que necesitas dos flujos de pago:
- iOS: StoreKit
- Android/Web: Stripe

El backend debe verificar ambos tipos de compra.
