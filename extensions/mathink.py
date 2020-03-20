import simplestyle
import subprocess
import sys
import os
import inkex
import re
import copy
# from inkex.elements import load_svg, Group

svgstr = ""

SVG_NS = u"http://www.w3.org/2000/svg"


def process_exists(process_name):
    call = 'TASKLIST', '/FI', 'imagename eq %s' % process_name
    # use buildin check_output right away
    output = subprocess.check_output(call)
    # check in last line for process name
    last_line = output.strip().split('\r\n')[-1]
    # because Fail message could be translated
    return last_line.lower().startswith(process_name.lower())

class Mathink(inkex.Effect):
    def __init__(self):
        inkex.Effect.__init__(self)
        self.OptionParser.add_option("--out",
                        action="store", type="string", 
                        dest="out", 
                        help="out svg file path")
        self.OptionParser.add_option(
            "-s", "--scale", action="store", type="float",
            dest="scale",
            default=1.0)

    def get_transform(self, scale):
        # return 'scale(%f,%f)' % (scale, scale)
        return  'matrix(%f,0,0,%f,0,0)' % (
                self.options.scale, self.options.scale)

    def effect(self):
        if not os.path.isfile(self.options.out):
            sys.exit()
        newnode = inkex.etree.parse(self.options.out).getroot()
        scale = self.options.scale
        
        # g = Group.create(self.options.parent, True)
        # inkex.debug(newnode)
        # g.append(newnode)
        # self.current_layer.append(g)
        # extDocument = load_svg(svgstr)
        svg = self.document.getroot()
        g = inkex.etree.SubElement(svg, 'g')        
        g.append(newnode)
        if scale is not None:
            g.attrib['transform'] = 'matrix(%f,0,0,%f,%f,%f)' % (
                self.options.scale, self.options.scale,
                self.view_center[0],
                self.view_center[1])
            #inkex.debug(g.attrib['transform'])
        # self.current_layer.append(g)
        


if __name__ == '__main__':
    
    argv = []
    output_file = "Mathink/out.svg"
    for arg in sys.argv[:]:
        if arg.startswith("--out="):
            output_file = arg.split("=")[1]
        else:
            argv.append(arg)
    argv.insert(0, output_file)
    
    if not process_exists("Mathink.GUI.exe"):
        subprocess.Popen("explorer.exe " + os.path.abspath(os.path.dirname(output_file) + "/Mathink.GUI.exe"))
        sys.exit()
    if not os.path.isfile(output_file):
        inkex.debug("file not found: " + output_file)
        sys.exit()
    # with open(output_file, 'r') as file:
    #     svgstr = file.read()

    e = Mathink()
    e.affect()
