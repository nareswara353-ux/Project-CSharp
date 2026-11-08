# Maintenance Mode Guide

Fitur **Maintenance Mode** memungkinkan admin memblokir sementara akses ke sebagian besar API saat sedang melakukan pemeliharaan sistem, tanpa harus mematikan server secara keseluruhan.

Fitur ini dikendalikan oleh **Feature Flag** bernama `MaintenanceMode`.

## 🔄 Cara Kerja

1. Middleware (`MaintenanceModeMiddleware`) mencegat setiap request.
2. Middleware mengecek nilai flag `MaintenanceMode` via `IFeatureFlagService`.
3. Jika flag **Aktif (true)**:
   - Request ke endpoint biasa akan diblokir dan dikembalikan **HTTP 503 Service Unavailable**.
   - Response berisi JSON dengan `errorCode: "MAINTENANCE_MODE"` dan header `Retry-After: 300` (detik).
4. Jika flag **Nonaktif (false)**, request diteruskan seperti biasa.

## 🛣️ Path yang Dikecualikan (Bypass)

Saat maintenance mode aktif, beberapa endpoint tetap bisa diakses agar admin bisa melakukan *recovery* atau mematikan maintenance:

- `/health` dan `/health/*` (untuk load balancer/kubernetes probe)
- `*/auth/*` (agar user tetap bisa login untuk mengakses endpoint admin)
- `*/featureflags*` (agar admin bisa menonaktifkan maintenance mode)
- `/swagger*` (untuk dokumentasi API)

## 🛠️ Cara Mengaktifkan / Menonaktifkan

Gunakan endpoint Feature Flags (memerlukan role `Admin`):

**Aktifkan Maintenance Mode:**
```bash
curl -X PUT https://localhost:5001/api/v1/featureflags/MaintenanceMode \
  -H "Authorization: Bearer <ADMIN_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"enabled": true}'
Nonaktifkan Maintenance Mode:

bash
curl -X PUT https://localhost:5001/api/v1/featureflags/MaintenanceMode \
  -H "Authorization: Bearer <ADMIN_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"enabled": false}'
⚠️ Catatan Penting
Middleware ini bersifat fail-open. Jika IFeatureFlagService gagal (misal koneksi Redis putus), request akan tetap diizinkan masuk agar sistem tidak lumpuh total.

Jika Anda menggunakan multiple instance (load balanced), pastikan flag disimpan di distributed store (misal Redis via RedisFeatureFlagService, atau database) agar semua instance membaca nilai yang sama.
