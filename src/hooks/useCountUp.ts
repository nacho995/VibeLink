import { useEffect, useRef, useState } from 'react';

interface UseCountUpOptions {
  readonly duration?: number; // milliseconds
  readonly start?: number;
}

export function useCountUp(target: number, options: UseCountUpOptions = {}): number {
  const { duration = 1200, start = 0 } = options;
  const [value, setValue] = useState(start);
  const frame = useRef<number>();
  const startTime = useRef<number>();

  useEffect(() => {
    cancelAnimationFrame(frame.current ?? 0);
    startTime.current = undefined;

    const step = (timestamp: number): void => {
      if (startTime.current === undefined) startTime.current = timestamp;
      const progress = Math.min((timestamp - startTime.current) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3); // ease-out cubic
      setValue(start + (target - start) * eased);
      if (progress < 1) frame.current = requestAnimationFrame(step);
    };

    frame.current = requestAnimationFrame(step);
    return () => cancelAnimationFrame(frame.current ?? 0);
  }, [target, duration, start]);

  return value;
}
