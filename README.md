# ⚡ FitMetrics

**Yapay zekâ destekli beslenme, antrenman ve sağlık takip platformu.**

Kullanıcıların beslenme, egzersiz ve vücut ölçülerini tek platformda toplayıp; kalori dengesi, makro dağılımı, kilo trendi ve antrenman dengesi üzerine **kişiselleştirilmiş analizler** üreten full-stack bir web uygulaması.

> Clean Architecture + SOLID prensipleriyle, ASP.NET Core 9 Web API ve React + TypeScript ile geliştirilmiştir.

---

## 🚀 Özellikler

- **Kimlik doğrulama**: JWT tabanlı kayıt / giriş, BCrypt ile parola hash'leme
- **Akıllı hedefler**: Kayıtta Mifflin-St Jeor (BMR) + aktivite + hedefe göre otomatik günlük kalori & protein hedefi
- **Beslenme takibi**: **FatSecret** ile gerçek besin arama + barkod, gram bazlı kalori/makro hesabı, öğün gruplama (kahvaltı/öğle/akşam/ara)
- **Antrenman takibi**: 24 egzersizlik katalog (kas grubu + kategori), set/tekrar/ağırlık veya süre girişi, otomatik kalori yakım hesabı
- **İlerleme takibi**: Kilo ve vücut yağı geçmişi, Chart.js grafikleri
- **AI Insights motoru**: Son 14–21 günü analiz ederek üretilen öneriler
  - Kalori dengesi (açık/fazla → aylık yağ kaybı/artış projeksiyonu)
  - Protein yeterliliği (hedefe göre % sapma)
  - Makro dağılımı
  - Antrenman dengesi (ihmal edilen kas grubu tespiti)
  - Kilo trendi (hedefe göre haftalık değişim)
  - Kayıt tutarlılığı
- **Dashboard**: Özet kartları + 14 günlük kalori trendi + makro dağılımı + kilo grafiği

---

## 🏗️ Mimari (Clean Architecture)

```
FitMetrics/
├── backend/
│   ├── FitMetrics.Domain          # Entity'ler, enum'lar (bağımlılık yok)
│   ├── FitMetrics.Application      # DTO, servisler, validation, AI motoru, arayüzler
│   ├── FitMetrics.Infrastructure   # EF Core, DbContext, JWT, BCrypt, migration
│   └── FitMetrics.API              # Controller'lar, middleware, DI, Swagger
└── frontend/                       # React + TypeScript + Tailwind + Chart.js
```

**Bağımlılık akışı:** `API → Application → Domain` ve `API → Infrastructure → Application → Domain`
Application katmanı, EF Core'a `IApplicationDbContext` soyutlaması üzerinden bağlıdır; somut `AppDbContext` Infrastructure'dadır.

---

## 🛠️ Teknolojiler

| Katman | Teknolojiler |
| --- | --- |
| Backend | ASP.NET Core 9 Web API, EF Core 9, SQL Server (LocalDB) |
| Kimlik | JWT (HMAC-SHA256), BCrypt.Net |
| Eşleme / Doğrulama | Mapster, FluentValidation |
| Frontend | React 19, TypeScript, Vite, Tailwind CSS v4 |
| Grafik / HTTP | Chart.js + react-chartjs-2, Axios, React Router |
| API Dokümantasyonu | Swagger / OpenAPI (Swashbuckle) |
| AI / Vision | Claude API (Anthropic Messages API, raw HttpClient) |
| PDF Rapor | QuestPDF (Community) |
| Besin arama | FatSecret Platform API (OAuth2) |
| Barkod | OpenFoodFacts API + @zxing/browser (kamera tarama) |

> **Not:** Spec'te AutoMapper belirtilmişti; ancak AutoMapper v15+ ticari lisansa geçti ve ücretsiz sürümlerde yüksek önem dereceli bir güvenlik açığı (GHSA-rvv3-g6hj-g44x) bulunuyor. Bu nedenle MIT lisanslı, daha hızlı ve birebir muadili olan **Mapster** tercih edildi.

---

## 📋 Gereksinimler

- [.NET SDK 9.x](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- SQL Server **LocalDB** (`(localdb)\MSSQLLocalDB`) — Visual Studio veya SQL Server Express ile gelir

---

## ▶️ Çalıştırma

### 1) Backend (API)

```bash
cd backend/FitMetrics.API
dotnet run
```

- API: **http://localhost:5072**
- Swagger UI: **http://localhost:5072/swagger**
- Veritabanı ve egzersiz seed verisi uygulama açılışında otomatik oluşturulur (`Database.Migrate()`). Besinler FatSecret aramasıyla içe aktarılır (statik besin seed'i yoktur).

### 2) Frontend

```bash
cd frontend
npm install
npm run dev
```

- Uygulama: **http://localhost:5173**
- Vite, `/api` isteklerini otomatik olarak backend'e (`localhost:5072`) yönlendirir (CORS gerektirmez).

İlk kullanım: **Kayıt ol** → profil/hedef bilgilerini gir → beslenme & antrenman ekle → **Dashboard** ve **AI Insights** sayfalarında analizleri gör.

### 3) AI özellikleri (opsiyonel)

AI Koç sayfası (akıllı öğün planı, doğal dil koçluk, fotoğraftan yemek tanıma) **Claude API** kullanır. Anahtar olmadan uygulamanın geri kalanı tam çalışır; AI özellikleri zarifçe devre dışı kalır. Etkinleştirmek için bir Anthropic API anahtarı tanımla:

```bash
# Ortam değişkeni (önerilen)
setx ANTHROPIC_API_KEY "sk-ant-..."   # Windows; yeni terminal aç
```

veya `backend/FitMetrics.API/appsettings.json` → `Anthropic:ApiKey`. Varsayılan model `claude-opus-4-8`; maliyet için `Anthropic:Model` değerini `claude-sonnet-4-6` yapabilirsin.

**PDF rapor** (İlerleme sayfası → "📄 PDF Rapor") anahtar gerektirmez; QuestPDF ile sunucuda üretilir.

### 4) FatSecret (besin arama)

Beslenme sayfasındaki **🔎 Besin Ara** özelliği FatSecret Platform API kullanır. Kimlik bilgileri **repo'ya yazılmaz**, `dotnet user-secrets` ile saklanır:

```bash
cd backend/FitMetrics.API
dotnet user-secrets set "FatSecret:ClientId" "<client-id>"
dotnet user-secrets set "FatSecret:ClientSecret" "<client-secret>"
```

> ⚠️ **IP whitelist:** FatSecret, API çağrılarını yalnızca panelde tanımlı IP'lerden kabul eder. FatSecret hesabında **Manage API Keys → IP Restrictions** altında sunucunun genel IP'sini eklemen gerekir; aksi halde arama `Invalid IP address` hatası döner.

---

## 🔌 Başlıca API Uç Noktaları

| Method | Endpoint | Açıklama |
| --- | --- | --- |
| POST | `/api/auth/register` | Kayıt (otomatik hedef hesaplama) |
| POST | `/api/auth/login` | Giriş (JWT) |
| GET | `/api/auth/me` | Mevcut kullanıcı |
| GET/PUT | `/api/profile` | Profil görüntüle / güncelle |
| GET | `/api/nutrition/foods` | Besin kataloğu (arama destekli) |
| POST/DELETE | `/api/nutrition/logs` | Öğün ekle / sil |
| GET | `/api/nutrition/summary?date=` | Günlük beslenme özeti |
| GET | `/api/workout/exercises` | Egzersiz kataloğu |
| POST/GET/DELETE | `/api/workout/logs` | Antrenman ekle / listele / sil |
| POST/GET/DELETE | `/api/weight` | Kilo kaydı ekle / geçmiş / sil |
| GET | `/api/dashboard` | Dashboard verisi |
| GET | `/api/insights` | Kural tabanlı AI analizleri |
| GET | `/api/ai/status` | AI etkin mi (API anahtarı var mı) |
| POST | `/api/ai/meal-plan` | Claude ile akıllı öğün/program oluşturma |
| GET | `/api/ai/coach` | Claude ile doğal dil koçluk |
| POST | `/api/ai/analyze-meal-photo` | Claude vision ile fotoğraftan kalori/makro tahmini |
| POST | `/api/ai/chat` | Verilere dayalı çok-turlu sohbet asistanı (Claude) |
| GET | `/api/reports/monthly` | PDF aylık ilerleme raporu |
| GET | `/api/nutrition/barcode/{code}` | Barkoddan besin (OpenFoodFacts) |
| POST | `/api/dietitian/enroll` | Diyetisyen moduna geç |
| GET/POST | `/api/dietitian/clients` | Danışan listele / ekle |
| DELETE | `/api/dietitian/clients/{id}` | Danışan bağını kaldır |
| GET | `/api/dietitian/clients/{id}/dashboard` | Danışan paneli (yetkili) |
| GET | `/api/dietitian/clients/{id}/insights` | Danışan AI analizleri (yetkili) |

Korumalı tüm uç noktalar `Authorization: Bearer <token>` başlığı bekler.

---

## 🧠 AI Insights Motoru Nasıl Çalışır?

`InsightService`, kural tabanlı bir analiz motorudur (deterministik, açıklanabilir):

1. Son 14 günün beslenme, 21 günün antrenman ve tüm kilo kayıtlarını çeker.
2. TDEE'yi (Mifflin-St Jeor × aktivite faktörü) hesaplar.
3. Ortalama günlük alım, protein yeterliliği, makro yüzdeleri, kas grubu dağılımı ve kilo trendini değerlendirir.
4. Kullanıcının hedefine (kilo verme / koruma / kas kazanma) göre olumlu/bilgi/uyarı seviyesinde, sayısal temelli öneriler üretir.

> Yapı, ileride bir LLM (ör. Claude API) entegrasyonuyla doğal dil önerilerine genişletilebilecek şekilde soyutlanmıştır.

---

## 🗺️ Yol Haritası (Premium)

**Tamamlandı (Faz 2):**
- [x] LLM destekli akıllı öğün/program oluşturucu (Claude)
- [x] Doğal dil kişisel koçluk (Claude)
- [x] Fotoğraftan yemek tanıma (Claude vision)
- [x] PDF aylık ilerleme raporu (QuestPDF)

**Tamamlandı (Faz 3):**
- [x] Barkod ile ürün ekleme (OpenFoodFacts + kamera tarama)
- [x] Diyetisyen/antrenör paneli (rol tabanlı, danışan takibi + yetkilendirme)

**Tamamlandı (Faz 5):**
- [x] AI Asistan: verilere dayalı çok-turlu sohbet botu (Claude, bağlam enjeksiyonu)

**Gelecek fikirleri:**
- [ ] Push bildirimleri, çoklu dil, mobil uygulama

---

## 📄 Lisans

Eğitim ve portföy amaçlı geliştirilmiştir.
