import numpy as np
import cv2

import tensorflow as tf
import os

graph_def = tf.compat.v1.GraphDef()
labels = []

# These are set to the default names from exported models, update as needed.
filename = "C://Users//user//Desktop//zzzzzzzzz.pb"
#C:\Users\user\Desktop\20191108124036754.jpg


# Import the TF graph
with tf.io.gfile.GFile(filename, 'rb') as f:
    graph_def.ParseFromString(f.read())
    tf.import_graph_def(graph_def, name='')


output_layer = 'LightVTNet/output:0'
input_node = 'LightVTNet/X:0'
phase_node = 'LightVTNet/phase:0'


image = cv2.imread("C://Users//user//Desktop//20191108124036754.jpg")
image = np.reshape(image,(100*100*3))

with tf.compat.v1.Session() as sess:
    try:
        prob_tensor = sess.graph.get_tensor_by_name(output_layer)
        predictions, = sess.run(prob_tensor, {input_node: [image],  phase_node: False})
        print(predictions)
    except KeyError:
        print("Couldn't find classification output layer: " + output_layer + ".")
        print("Verify this a model exported from an Object Detection project.")
        exit(-1)