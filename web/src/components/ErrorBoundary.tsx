import { Component } from 'react';
import type { ReactNode, ErrorInfo } from 'react';
import { MaintenancePage } from '../pages/MaintenancePage';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('ErrorBoundary caught:', error, info.componentStack);
  }

  render() {
    if (this.state.hasError) {
      return <MaintenancePage onRetry={() => this.setState({ hasError: false })} />;
    }
    return this.props.children;
  }
}
