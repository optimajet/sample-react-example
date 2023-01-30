import React, {useRef, useState} from "react";
import {Button, Container, Modal} from "rsuite";
import WorkflowDesigner from "@optimajet/workflow-designer-react";
import settings from "./settings";
import SchemeMenu from "./SchemeMenu";
import ProcessMenu from "./ProcessMenu";
import ProcessParameters from "./ProcessParameters";
import ProcessConsoleWindow from "./ProcessConsoleWindow";

const Designer = (props) => {
    const {schemeCode, ...otherProps} = {props}
    const [code, setCode] = useState(props.schemeCode)
    const [processId, setProcessId] = useState(props.processId)
    const [processParametersState, setProcessParametersState] = useState({
        open: false,
        defaultParameters: []
    })
    const [processParameters, setProcessParameters] = useState([])
    const designerRef = useRef()

    const designerConfig = {
        renderTo: 'wfdesigner',
        apiurl: settings.designerUrl,
        templatefolder: '/templates/',
        widthDiff: 300,
        heightDiff: 100,
        showSaveButton: !processId
    };

    const createOrLoad = (code) => {
        setCode(code)
        setProcessId(null)
        const data = {
            schemecode: code,
            processid: undefined
        }
        const wfDesigner = designerRef.current.innerDesigner;
        if (wfDesigner.exists(data)) {
            wfDesigner.load(data);
        } else {
            wfDesigner.create(code);
        }
    }

    const refreshDesigner = () => {
        designerRef.current.loadScheme();
    }

    const onOpenProcessWindow = () => {
        fetch(`${settings.workflowUrl}/schemeParameters/${code}`)
            .then(result => result.json())
            .then(data => {
                setProcessParametersState({
                    open: true,
                    defaultParameters: data
                })
            })
    };
    const onCloseProcessWindow = () => setProcessParametersState({...processParametersState, open: false});

    const onCreateProcess = () => {
        fetch(`${settings.workflowUrl}/createInstance/${code}`,
            {
                method: "post",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    processParameters: processParameters
                })
            })
            .then(result => result.json())
            .then(data => {
                setProcessId(data.id)
                const params = {
                    schemecode: code,
                    processid: data.id
                };
                designerRef.current.innerDesigner.load(params,
                    () => console.log('Process loaded'),
                    error => console.error(error));
                setProcessParametersState({...processParametersState, open: false});
            });
    }

    return <div style={{display: "flex"}}><Container style={{maxWidth: '80%', overflow: 'hidden'}}>
        {!processId &&
            <SchemeMenu {...otherProps} schemeCode={code}
                        onNewScheme={createOrLoad} onCreateProcess={onOpenProcessWindow}/>
        }
        {!!processId && <ProcessMenu processId={processId} afterCommandExecuted={refreshDesigner}/>}
        <Modal open={processParametersState.open} onClose={onCloseProcessWindow} overflow={true}>
            <Modal.Header>
                <Modal.Title>Initial process parameters</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <ProcessParameters onParametersChanged={(processParameters) => setProcessParameters(processParameters)}
                                   defaultParameters={processParametersState.defaultParameters}/>
            </Modal.Body>
            <Modal.Footer>
                <Button onClick={onCreateProcess} appearance="primary">
                    Create Process
                </Button>
                <Button onClick={onCloseProcessWindow} appearance="subtle">
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
        <WorkflowDesigner
            schemeCode={code}
            processId={processId}
            designerConfig={designerConfig}
            ref={designerRef}
        />
    </Container>
        {processId && <ProcessConsoleWindow processId={processId}/>}
    </div>
}

export default Designer;
