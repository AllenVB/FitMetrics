import { useRef, useState, type FormEvent } from 'react';
import type { IScannerControls } from '@zxing/browser';
import { nutritionApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, ErrorAlert, Icon, Input } from './ui';
import type { BarcodeLookupResult, Food } from '../types';

export default function BarcodeAdd({ onFoodCreated }: { onFoodCreated: (food: Food) => void }) {
  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [adding, setAdding] = useState(false);
  const [scanning, setScanning] = useState(false);
  const [error, setError] = useState('');
  const [result, setResult] = useState<BarcodeLookupResult | null>(null);

  const videoRef = useRef<HTMLVideoElement>(null);
  const controlsRef = useRef<IScannerControls | null>(null);

  const lookup = async (value: string) => {
    const c = value.trim();
    if (!c) return;
    setLoading(true);
    setError('');
    setResult(null);
    try {
      setResult(await nutritionApi.lookupBarcode(c));
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    lookup(code);
  };

  const startScan = async () => {
    setError('');
    setScanning(true);
    try {
      // zxing yalnızca kamera kullanılınca yüklenir (ana paketi şişirmemek için lazy import)
      const { BrowserMultiFormatReader } = await import('@zxing/browser');
      const reader = new BrowserMultiFormatReader();
      controlsRef.current = await reader.decodeFromVideoDevice(undefined, videoRef.current!, (res, _err, controls) => {
        if (res) {
          const text = res.getText();
          setCode(text);
          controls.stop();
          controlsRef.current = null;
          setScanning(false);
          lookup(text);
        }
      });
    } catch {
      setError('Kameraya erişilemedi. Barkodu elle girebilirsin.');
      setScanning(false);
    }
  };

  const stopScan = () => {
    controlsRef.current?.stop();
    controlsRef.current = null;
    setScanning(false);
  };

  const addToCatalog = async () => {
    if (!result) return;
    setAdding(true);
    setError('');
    try {
      const food = await nutritionApi.createFood({
        name: result.name,
        brand: result.brand ?? null,
        category: 'Barkod',
        caloriesPer100g: result.caloriesPer100g,
        proteinPer100g: result.proteinPer100g,
        carbsPer100g: result.carbsPer100g,
        fatPer100g: result.fatPer100g,
      });
      onFoodCreated(food);
      setResult(null);
      setCode('');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setAdding(false);
    }
  };

  return (
    <Card className="mb-6">
      <h3 className="mb-3 flex items-center gap-2 font-semibold text-on-surface">
        <Icon name="barcode_scanner" className="text-primary" /> Barkod ile Ekle
      </h3>
      <form onSubmit={onSubmit} className="flex flex-wrap items-end gap-3">
        <div className="min-w-[200px] flex-1">
          <Input value={code} onChange={(e) => setCode(e.target.value)} placeholder="Barkod numarası (örn. 3017620422003)" />
        </div>
        <Button type="submit" variant="secondary" disabled={loading}>{loading ? 'Aranıyor…' : 'Ara'}</Button>
        {!scanning ? (
          <Button type="button" variant="secondary" onClick={startScan} className="flex items-center gap-1.5">
            <Icon name="photo_camera" className="text-base" /> Kamera
          </Button>
        ) : (
          <Button type="button" variant="ghost" onClick={stopScan}>Durdur</Button>
        )}
      </form>

      {scanning && (
        <video ref={videoRef} className="mt-3 max-h-52 w-full rounded-lg bg-black object-contain" />
      )}

      <div className="mt-3"><ErrorAlert message={error} /></div>

      {result && (
        <div className="mt-3 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-white/10 bg-surface-container-high p-3">
          <div>
            <div className="font-medium text-on-surface">
              {result.name}{result.brand ? <span className="text-on-surface-variant"> · {result.brand}</span> : null}
            </div>
            <div className="text-xs text-on-surface-variant">
              100g: {result.caloriesPer100g} kcal · P{result.proteinPer100g} K{result.carbsPer100g} Y{result.fatPer100g}
            </div>
          </div>
          <Button onClick={addToCatalog} disabled={adding}>{adding ? 'Ekleniyor…' : 'Kataloğa ekle ve seç'}</Button>
        </div>
      )}
    </Card>
  );
}
