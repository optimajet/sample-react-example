import {HubConnectionBuilder,LogLevel} from '@microsoft/signalr'
import settings from './settings';
import {createContext, useContext, useEffect, useRef, useState} from 'react';

const ProcessConsoleContext = createContext([]);

const ProcessConsoleProvider = ({processId, children}) => {
  const [consoleLines, setConsoleLines] = useState([]);
  const consoleLinesRef = useRef([]);

  useEffect(() => {
    const processConsoleConnection = new HubConnectionBuilder()
        .withUrl(`${settings.workflowUrl}/processConsole/`)
        .configureLogging(LogLevel.Debug)
        .build();
    processConsoleConnection.on('ReceiveMessage', function (message) {
      const parsedMessage = message;
      if (parsedMessage.processId !== processId) {
        return;
      }
      const nextConsoleLines = Object.assign([], consoleLinesRef.current);
      nextConsoleLines.unshift({
        date: new Date(),
        message: parsedMessage.message.replaceAll('\\n', '\n')
      });
      consoleLinesRef.current = nextConsoleLines.slice(nextConsoleLines.length - 101)
      setConsoleLines(consoleLinesRef.current);
    });
    const connect = async () => {
      await processConsoleConnection.start();
    }
    const disconnect = async () => {
      await processConsoleConnection.stop();
    }
    connect();
    return disconnect;
  }, [])

  return <ProcessConsoleContext.Provider value={consoleLines}>{children}</ProcessConsoleContext.Provider>
}

const useProcessConsoleContext = () => useContext(ProcessConsoleContext);

export  {ProcessConsoleProvider, useProcessConsoleContext};